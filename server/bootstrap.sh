#!/usr/bin/env bash
# bootstrap.sh — Full server setup for IranConnect Core API
# Run as root. Idempotent — safe to re-run on every deploy.
set -Eeuo pipefail

# ── colours ──────────────────────────────────────────────────────────────────
C_OK=$'\e[32m'; C_WARN=$'\e[33m'; C_ERR=$'\e[31m'; C_INFO=$'\e[36m'; C_RST=$'\e[0m'
log()  { printf '%s[*]%s %s\n' "$C_INFO" "$C_RST" "$*"; }
ok()   { printf '%s[+]%s %s\n' "$C_OK"   "$C_RST" "$*"; }
warn() { printf '%s[!]%s %s\n' "$C_WARN" "$C_RST" "$*" >&2; }
die()  { printf '%s[x]%s %s\n' "$C_ERR"  "$C_RST" "$*" >&2; exit 1; }
trap 'die "Failed at line $LINENO."' ERR

[[ "${EUID}" -eq 0 ]] || die "Must run as root."

# ── load CI-provided config ───────────────────────────────────────────────────
CICD_ENV="/etc/iranconnect/cicd.env"
[[ -f "$CICD_ENV" ]] || die "$CICD_ENV not found — workflow must write it before running this script."
# shellcheck disable=SC1090
source "$CICD_ENV"

DOMAIN="${DOMAIN:?DOMAIN not set in cicd.env}"
APP_URL="${APP_URL:?APP_URL not set in cicd.env}"
CERTBOT_EMAIL="${CERTBOT_EMAIL:-admin@packsi.net}"

APP_DIR=/opt/iranconnect
APP_USER=iranconnect
SERVICE_NAME=iranconnect
APP_PORT=5000
PG_DB=iranconnect
PG_USER=iranconnect
SECRETS_FILE=/etc/iranconnect/secrets.env
CONFIG_FILE=/etc/iranconnect/config.env

# ── 1. SYSTEM PACKAGES ───────────────────────────────────────────────────────
log "Installing system packages ..."
export DEBIAN_FRONTEND=noninteractive
apt-get update -y -qq
apt-get install -y -qq \
    curl wget gnupg2 apt-transport-https ca-certificates \
    software-properties-common lsb-release \
    nginx certbot python3-certbot-nginx \
    ufw postgresql postgresql-contrib \
    rsync openssl
ok "System packages installed."

# ── 2. .NET 8 ASP.NET CORE RUNTIME ───────────────────────────────────────────
if ! dotnet --list-runtimes 2>/dev/null | grep -q "Microsoft.AspNetCore.App 8\."; then
    log "Installing .NET 8 ASP.NET Core Runtime ..."
    OS_VER="$(lsb_release -rs)"
    PKG_URL="https://packages.microsoft.com/config/ubuntu/${OS_VER}/packages-microsoft-prod.deb"
    wget -q "$PKG_URL" -O /tmp/ms-prod.deb
    dpkg -i /tmp/ms-prod.deb
    apt-get update -y -qq
    apt-get install -y aspnetcore-runtime-8.0
    rm /tmp/ms-prod.deb
    ok ".NET 8 runtime installed."
else
    ok ".NET 8 runtime already present."
fi

# ── 3. AMNEZIAWG SETUP ────────────────────────────────────────────────────────
# AmneziaWG (awg) replaces plain WireGuard: raw WG handshakes are dropped by
# Iranian carrier/border DPI, so the tunnel never completes from mobile/abroad.
# The amneziawg script disables wg-quick@wg0, installs awg, and writes the
# obfuscation params (shared with the backend via appsettings.Production.json).
log "Running AmneziaWG setup ..."
chmod +x /tmp/setup-amneziawg-server.sh
bash /tmp/setup-amneziawg-server.sh
ok "AmneziaWG configured."

# ── 4. POSTGRESQL ─────────────────────────────────────────────────────────────
log "Configuring PostgreSQL ..."
systemctl enable postgresql --quiet
systemctl start postgresql

# Wait for PostgreSQL to be ready
for i in {1..15}; do
    pg_isready -q 2>/dev/null && break
    sleep 2
done
pg_isready -q || die "PostgreSQL not ready after 30s."

# Generate secrets on first deploy (never overwritten by CI)
if [[ ! -f "$SECRETS_FILE" ]]; then
    log "Generating application secrets (first deploy) ..."
    DB_PASS="$(openssl rand -hex 24)"
    JWT_KEY="$(openssl rand -hex 32)"
    mkdir -p /etc/iranconnect
    cat > "$SECRETS_FILE" << EOF
# Auto-generated on first deploy — do NOT edit manually unless you know what you're doing.
# DB_PASSWORD and Jwt__Secret are used across sessions; changing them invalidates all tokens and DB connections.
DB_PASSWORD=${DB_PASS}
JWT_SECRET=${JWT_KEY}
Jwt__Secret=${JWT_KEY}
ConnectionStrings__DefaultConnection=Host=127.0.0.1;Port=5432;Database=${PG_DB};Username=${PG_USER};Password=${DB_PASS}
EOF
    chown root:${APP_USER} "$SECRETS_FILE" 2>/dev/null || chown root:"$SECRETS_FILE" 2>/dev/null || true
    chmod 640 "$SECRETS_FILE"
    ok "Secrets file created: $SECRETS_FILE"
else
    ok "Secrets file already exists — preserving."
fi
# shellcheck disable=SC1090
source "$SECRETS_FILE"

# Create PostgreSQL role + DB (idempotent)
sudo -u postgres psql -tc "SELECT 1 FROM pg_roles WHERE rolname='${PG_USER}'" \
    | grep -q 1 \
    || sudo -u postgres psql -c "CREATE USER ${PG_USER} WITH PASSWORD '${DB_PASSWORD}';"
sudo -u postgres psql -c "ALTER USER ${PG_USER} WITH PASSWORD '${DB_PASSWORD}';" > /dev/null

sudo -u postgres psql -tc "SELECT 1 FROM pg_database WHERE datname='${PG_DB}'" \
    | grep -q 1 \
    || sudo -u postgres psql -c "CREATE DATABASE ${PG_DB} OWNER ${PG_USER};"

sudo -u postgres psql -c "GRANT ALL PRIVILEGES ON DATABASE ${PG_DB} TO ${PG_USER};" > /dev/null
ok "PostgreSQL: DB '${PG_DB}' and user '${PG_USER}' ready."

# ── 5. DEPLOY APPLICATION FILES ───────────────────────────────────────────────
log "Deploying application to ${APP_DIR} ..."

# Ensure system user (wireguard script creates it, but be safe)
id -u "$APP_USER" &>/dev/null \
    || useradd --system --no-create-home --shell /usr/sbin/nologin "$APP_USER"

mkdir -p "$APP_DIR"
rsync -a --delete /tmp/iranconnect-deploy/ "$APP_DIR/"
chown -R "${APP_USER}:${APP_USER}" "$APP_DIR"
chmod -R o-rwx "$APP_DIR"
ok "Application files deployed."

# ── 6. EF MIGRATIONS ──────────────────────────────────────────────────────────
if [[ -f "$APP_DIR/efbundle" ]]; then
    log "Running EF database migrations ..."
    chmod +x "$APP_DIR/efbundle"
    # Build connection string directly — do NOT use ${ConnectionStrings__DefaultConnection}
    # sourced from secrets.env because bash treats semicolons as command separators
    # and truncates the value. Systemd's env-file parser handles semicolons correctly.
    EF_CONN="Host=127.0.0.1;Port=5432;Database=${PG_DB};Username=${PG_USER};Password=${DB_PASSWORD}"
    if "$APP_DIR/efbundle" --connection "$EF_CONN"; then
        ok "Database migrations applied."
    else
        warn "EF bundle returned non-zero — check output above."
    fi
else
    warn "efbundle not found — skipping migrations."
fi

# ── 7. APP CONFIG ENV FILE ────────────────────────────────────────────────────
log "Writing application config ..."
cat > "$CONFIG_FILE" << EOF
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:${APP_PORT}
Jwt__Issuer=IranConnect
Jwt__Audience=IranConnectUsers
App__BaseUrl=${APP_URL}
EOF
chmod 644 "$CONFIG_FILE"
ok "Config file written: $CONFIG_FILE"

# ── 8. SYSTEMD SERVICE ────────────────────────────────────────────────────────
log "Writing systemd service ..."
cat > "/etc/systemd/system/${SERVICE_NAME}.service" << EOF
[Unit]
Description=IranConnect Core API
After=network.target postgresql.service wg-quick@wg0.service
Wants=wg-quick@wg0.service

[Service]
Type=exec
User=${APP_USER}
WorkingDirectory=${APP_DIR}
ExecStart=/usr/bin/dotnet ${APP_DIR}/IranConnect.API.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=${SERVICE_NAME}
EnvironmentFile=/etc/iranconnect/secrets.env
EnvironmentFile=/etc/iranconnect/config.env

[Install]
WantedBy=multi-user.target
EOF

systemctl daemon-reload
systemctl enable "$SERVICE_NAME" --quiet
ok "Systemd service configured."

# ── 9. UFW FIREWALL ───────────────────────────────────────────────────────────
log "Configuring UFW ..."
ufw allow 80/tcp  comment 'HTTP (ACME + redirect)' > /dev/null
ufw allow 443/tcp comment 'HTTPS'                  > /dev/null
ok "UFW: ports 80, 443 open."

# ── 10. NGINX ─────────────────────────────────────────────────────────────────
log "Configuring Nginx ..."
NGINX_CONF="/etc/nginx/sites-available/${SERVICE_NAME}"
ACME_ROOT="/var/www/certbot"
mkdir -p "$ACME_ROOT"

# HTTP-only config — used for ACME challenge and initial cert acquisition
cat > "$NGINX_CONF" << 'NGINXEOF'
server {
    listen 80;
    listen [::]:80;
    server_name __DOMAIN__;

    location /.well-known/acme-challenge/ {
        root /var/www/certbot;
    }

    location / {
        proxy_pass         http://127.0.0.1:__PORT__;
        proxy_http_version 1.1;
        proxy_set_header   Upgrade           $http_upgrade;
        proxy_set_header   Connection        keep-alive;
        proxy_set_header   Host              $host;
        proxy_set_header   X-Real-IP         $remote_addr;
        proxy_set_header   X-Forwarded-For   $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
        proxy_read_timeout 300;
    }
}
NGINXEOF

sed -i "s/__DOMAIN__/${DOMAIN}/g; s/__PORT__/${APP_PORT}/g" "$NGINX_CONF"

ln -sf "$NGINX_CONF" "/etc/nginx/sites-enabled/${SERVICE_NAME}"
rm -f /etc/nginx/sites-enabled/default
nginx -t && systemctl enable nginx --quiet && systemctl restart nginx
ok "Nginx (HTTP) configured and running."

# ── 11. SSL CERTIFICATE ────────────────────────────────────────────────────────
CERT_DIR="/etc/letsencrypt/live/${DOMAIN}"
if [[ ! -d "$CERT_DIR" ]]; then
    log "Obtaining SSL certificate for ${DOMAIN} via certbot ..."

    # Start app first so ACME challenge path is reachable through nginx
    systemctl restart "$SERVICE_NAME" || true
    sleep 3

    certbot certonly \
        --webroot \
        --webroot-path "$ACME_ROOT" \
        -d "$DOMAIN" \
        --non-interactive \
        --agree-tos \
        --email "$CERTBOT_EMAIL" \
        --rsa-key-size 4096

    ok "SSL certificate obtained."
else
    ok "SSL certificate already exists — skipping certbot."
fi

# Write full HTTPS config (after cert is confirmed present)
if [[ -d "$CERT_DIR" ]]; then
    cat > "$NGINX_CONF" << 'NGINXEOF'
server {
    listen 80;
    listen [::]:80;
    server_name __DOMAIN__;

    location /.well-known/acme-challenge/ {
        root /var/www/certbot;
    }

    location / {
        return 301 https://$host$request_uri;
    }
}

server {
    listen 443      ssl http2;
    listen [::]:443 ssl http2;
    server_name __DOMAIN__;

    ssl_certificate     /etc/letsencrypt/live/__DOMAIN__/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/__DOMAIN__/privkey.pem;
    ssl_session_timeout 1d;
    ssl_session_cache   shared:MozSSL:10m;
    ssl_protocols       TLSv1.2 TLSv1.3;
    ssl_ciphers         ECDHE-ECDSA-AES128-GCM-SHA256:ECDHE-RSA-AES128-GCM-SHA256:ECDHE-ECDSA-AES256-GCM-SHA384:ECDHE-RSA-AES256-GCM-SHA384:ECDHE-ECDSA-CHACHA20-POLY1305:ECDHE-RSA-CHACHA20-POLY1305;
    ssl_prefer_server_ciphers off;
    ssl_stapling        on;
    ssl_stapling_verify on;

    add_header Strict-Transport-Security "max-age=63072000; includeSubDomains; preload" always;
    add_header X-Frame-Options            DENY;
    add_header X-Content-Type-Options     nosniff;
    add_header X-XSS-Protection           "1; mode=block";

    location / {
        proxy_pass         http://127.0.0.1:__PORT__;
        proxy_http_version 1.1;
        proxy_set_header   Upgrade           $http_upgrade;
        proxy_set_header   Connection        keep-alive;
        proxy_set_header   Host              $host;
        proxy_set_header   X-Real-IP         $remote_addr;
        proxy_set_header   X-Forwarded-For   $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
        proxy_read_timeout  300;
        proxy_connect_timeout 75;
        proxy_send_timeout  300;
    }
}
NGINXEOF

    sed -i "s/__DOMAIN__/${DOMAIN}/g; s/__PORT__/${APP_PORT}/g" "$NGINX_CONF"
    nginx -t && systemctl reload nginx
    ok "Nginx (HTTPS) configured."

    # Setup certbot auto-renewal
    systemctl enable certbot.timer --quiet 2>/dev/null \
        || (crontab -l 2>/dev/null; echo "0 3 * * * certbot renew --quiet --nginx") | crontab -
fi

# ── 12. START / RESTART SERVICE ───────────────────────────────────────────────
log "Starting ${SERVICE_NAME} service ..."
systemctl restart "$SERVICE_NAME"

# Wait for service to come up
for i in {1..15}; do
    systemctl is-active --quiet "$SERVICE_NAME" && break
    sleep 2
done

if ! systemctl is-active --quiet "$SERVICE_NAME"; then
    die "Service failed to start. Logs: journalctl -u ${SERVICE_NAME} -n 50"
fi
ok "Service ${SERVICE_NAME} is running."

# ── 13. HEALTH CHECK ──────────────────────────────────────────────────────────
sleep 3
SWAGGER_URL="https://${DOMAIN}/swagger/index.html"
HTTP_STATUS="$(curl -sSo /dev/null -w "%{http_code}" "$SWAGGER_URL" --max-time 20 2>/dev/null || echo "000")"
if [[ "$HTTP_STATUS" == "200" ]]; then
    ok "Health check passed — Swagger responding (HTTP ${HTTP_STATUS})"
else
    warn "Health check: Swagger returned HTTP ${HTTP_STATUS} — may still be starting up."
fi

# ── DONE ──────────────────────────────────────────────────────────────────────
echo
ok "=========================================="
ok " DEPLOYMENT COMPLETE"
echo "  URL    : https://${DOMAIN}"
echo "  Swagger: https://${DOMAIN}/swagger"
echo "  Logs   : journalctl -u ${SERVICE_NAME} -f"
echo "  WG     : wg show wg0"
ok "=========================================="
