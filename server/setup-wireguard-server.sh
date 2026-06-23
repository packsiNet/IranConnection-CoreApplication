#!/usr/bin/env bash
#
# setup-wireguard-server.sh
# -----------------------------------------------------------------------------
# Complete, idempotent WireGuard server bootstrap for IranConnection backend.
#
# What it does:
#   1. Verifies root + supported Linux distro.
#   2. Installs WireGuard, UFW, iptables, curl, jq, iproute2.
#   3. Generates server key pair (reused if already present).
#   4. Detects public IP and default egress interface.
#   5. Enables IPv4/IPv6 forwarding + NAT masquerade.
#   6. Writes /etc/wireguard/wg0.conf (Interface only — no static Peer).
#      Backend adds/removes peers dynamically via `wg set`.
#   7. Opens WireGuard UDP port + SSH on UFW, enables forwarding.
#   8. Creates system user `iranconnect` and sudoers rule so the
#      .NET backend process can run `wg` / `wg-quick` without full root.
#   9. Writes /etc/iranconnect/appsettings.Production.json with server
#      public key + endpoint so the backend picks them up automatically.
#
# Routing model:
#   AllowedIPs in client configs = 0.0.0.0/0, ::/0 (full tunnel).
#   Split-tunneling (per-app) is enforced on the Android side via the
#   IranConnection app's VpnService allowed-apps list — not server-side.
#   The server NAT masquerades tunnel traffic out to the real internet.
#
# Usage:
#   sudo bash setup-wireguard-server.sh
#
# Environment overrides (all optional):
#   WG_PORT      WireGuard listen port   (default: 51820)
#   WG_NET       Tunnel subnet           (default: 10.0.0.0/24)
#   WG_DNS       DNS pushed to clients   (default: 1.1.1.1)
#   PUBLIC_IP    Force public IP (skip detection)
#   SSH_PORT     SSH port kept open on UFW (default: 22)
#   APP_USER     System user for .NET backend (default: iranconnect)
# -----------------------------------------------------------------------------

set -Eeuo pipefail

# ----------------------------- configuration ---------------------------------
WG_IFACE="wg0"
WG_PORT="${WG_PORT:-51820}"
# Subnet MUST match VpnConfigService.cs IP allocation (10.0.0.2 … 10.0.1.254)
WG_NET="${WG_NET:-10.0.0.0/24}"
WG_DNS="${WG_DNS:-1.1.1.1}"
WG_DIR="/etc/wireguard"
SSH_PORT="${SSH_PORT:-22}"
APP_USER="${APP_USER:-iranconnect}"
BACKEND_CFG_DIR="/etc/iranconnect"

# Derive server tunnel IP (.1 of the subnet)
WG_BASE="${WG_NET%.*}"          # e.g. 10.0.0
WG_SERVER_IP="${WG_BASE}.1"
WG_NET_PREFIX="${WG_NET##*/}"   # e.g. 24

# ----------------------------- pretty logging --------------------------------
if [[ -t 1 ]]; then
  C_OK=$'\e[32m'; C_WARN=$'\e[33m'; C_ERR=$'\e[31m'; C_INFO=$'\e[36m'; C_RST=$'\e[0m'
else
  C_OK=""; C_WARN=""; C_ERR=""; C_INFO=""; C_RST=""
fi
log()  { printf '%s[*]%s %s\n' "$C_INFO" "$C_RST" "$*"; }
ok()   { printf '%s[+]%s %s\n' "$C_OK"   "$C_RST" "$*"; }
warn() { printf '%s[!]%s %s\n' "$C_WARN" "$C_RST" "$*" >&2; }
die()  { printf '%s[x]%s %s\n' "$C_ERR"  "$C_RST" "$*" >&2; exit 1; }

trap 'die "Failed at line $LINENO. See output above."' ERR

# ----------------------------- pre-flight checks -----------------------------
[[ "${EUID}" -eq 0 ]] || die "Run as root:  sudo bash $0"

[[ -r /etc/os-release ]] || die "Cannot read /etc/os-release; unsupported OS."
# shellcheck disable=SC1091
. /etc/os-release
log "Detected OS: ${PRETTY_NAME:-$ID}"

case "${ID:-}${ID_LIKE:-}" in
  *debian*|*ubuntu*) PKG="apt"  ;;
  *rhel*|*fedora*|*centos*|*rocky*|*alma*) PKG="dnf" ;;
  *) die "Unsupported distro '${ID:-unknown}'. Supported: Debian/Ubuntu, RHEL/Fedora." ;;
esac

if ! modprobe wireguard 2>/dev/null && [[ ! -d /sys/module/wireguard ]]; then
  warn "wireguard kernel module not loaded yet; package install will pull wireguard-dkms if needed."
fi

# ----------------------------- install packages ------------------------------
install_packages() {
  log "Installing prerequisites via ${PKG} ..."
  export DEBIAN_FRONTEND=noninteractive
  if [[ "$PKG" == "apt" ]]; then
    apt-get update -y
    apt-get install -y wireguard wireguard-tools ufw iptables curl jq iproute2
  else
    dnf install -y epel-release || true
    dnf install -y wireguard-tools ufw iptables curl jq iproute || \
      die "Package install failed. Ensure EPEL/repos are reachable."
  fi
  ok "Packages installed."
}
install_packages

command -v wg       >/dev/null || die "wg not found after install."
command -v wg-quick >/dev/null || die "wg-quick not found after install."
command -v jq       >/dev/null || die "jq not found after install."

# ----------------------------- key generation --------------------------------
umask 077
mkdir -p "$WG_DIR"

gen_key_pair() {
  local name="$1"
  if [[ -s "${WG_DIR}/${name}_private" && -s "${WG_DIR}/${name}_public" ]]; then
    log "Re-using existing ${name} keys."
  else
    log "Generating ${name} key pair ..."
    wg genkey | tee "${WG_DIR}/${name}_private" | wg pubkey > "${WG_DIR}/${name}_public"
  fi
  chmod 600 "${WG_DIR}/${name}_private" "${WG_DIR}/${name}_public"
}

# Server keypair only — client peers are created dynamically by the backend
gen_key_pair server

SERVER_PRIV="$(cat "${WG_DIR}/server_private")"
SERVER_PUB="$(cat "${WG_DIR}/server_public")"

# ----------------------------- network detection -----------------------------
detect_public_ip() {
  if [[ -n "${PUBLIC_IP:-}" ]]; then echo "$PUBLIC_IP"; return; fi
  local ip=""
  for url in "https://api.ipify.org" "https://ifconfig.me/ip" "https://ipinfo.io/ip"; do
    ip="$(curl -fsS --max-time 8 "$url" 2>/dev/null | tr -d '[:space:]')" || true
    [[ "$ip" =~ ^[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+$ ]] && { echo "$ip"; return; }
  done
  die "Could not auto-detect public IP. Re-run with PUBLIC_IP=<your.ip> ."
}
PUBLIC_IP="$(detect_public_ip)"
ok "Public IP: ${PUBLIC_IP}"

WAN_IFACE="$(ip -4 route show default | awk '/default/ {print $5; exit}')"
[[ -n "$WAN_IFACE" ]] || die "Cannot determine default network interface."
ok "WAN interface: ${WAN_IFACE}"

# ----------------------------- IP forwarding ---------------------------------
log "Enabling IP forwarding ..."
cat > /etc/sysctl.d/99-wireguard-forward.conf <<EOF
net.ipv4.ip_forward = 1
net.ipv6.conf.all.forwarding = 1
EOF
sysctl --system >/dev/null
ok "IP forwarding enabled."

# ----------------------------- wg0.conf --------------------------------------
# [Interface] only — no static [Peer] block.
# The .NET backend adds peers dynamically:
#   wg set wg0 peer <pubkey> allowed-ips <client-ip>/32
#   wg-quick save wg0
log "Writing ${WG_DIR}/${WG_IFACE}.conf ..."
cat > "${WG_DIR}/${WG_IFACE}.conf" <<EOF
# Managed by setup-wireguard-server.sh + IranConnection backend.
# Do NOT add static [Peer] blocks here — the backend manages peers dynamically.
[Interface]
Address    = ${WG_SERVER_IP}/${WG_NET_PREFIX}
ListenPort = ${WG_PORT}
PrivateKey = ${SERVER_PRIV}

# NAT: masquerade tunnel traffic out via WAN interface so clients reach internet.
# Routing model: client AllowedIPs = 0.0.0.0/0 (full tunnel); per-app split
# tunneling is handled by the Android app's VpnService allowed-apps list.
PostUp   = iptables -A FORWARD -i %i -j ACCEPT; \
           iptables -A FORWARD -o %i -j ACCEPT; \
           iptables -t nat -A POSTROUTING -o ${WAN_IFACE} -j MASQUERADE
PostDown = iptables -D FORWARD -i %i -j ACCEPT; \
           iptables -D FORWARD -o %i -j ACCEPT; \
           iptables -t nat -D POSTROUTING -o ${WAN_IFACE} -j MASQUERADE
EOF
chmod 600 "${WG_DIR}/${WG_IFACE}.conf"
ok "wg0.conf written (Interface only)."

# ----------------------------- UFW firewall ----------------------------------
log "Configuring UFW ..."
ufw allow "${SSH_PORT}/tcp" comment 'SSH' >/dev/null || true
ufw allow "${WG_PORT}/udp" comment 'WireGuard' >/dev/null

if ! grep -q '^DEFAULT_FORWARD_POLICY="ACCEPT"' /etc/default/ufw 2>/dev/null; then
  sed -i 's/^DEFAULT_FORWARD_POLICY=.*/DEFAULT_FORWARD_POLICY="ACCEPT"/' /etc/default/ufw
fi

if ufw status | grep -q "Status: inactive"; then
  log "Enabling UFW (SSH on ${SSH_PORT}/tcp is already allowed — connection is safe)."
  ufw --force enable >/dev/null
else
  ufw reload >/dev/null || true
fi
ok "UFW: SSH ${SSH_PORT}/tcp + WireGuard ${WG_PORT}/udp open; forwarding ACCEPT."

# ----------------------------- start service ---------------------------------
log "Starting WireGuard service ..."
systemctl enable "wg-quick@${WG_IFACE}" >/dev/null 2>&1 || true
systemctl restart "wg-quick@${WG_IFACE}"
sleep 1
if ! wg show "${WG_IFACE}" >/dev/null 2>&1; then
  die "wg-quick@${WG_IFACE} failed to come up. Check: journalctl -u wg-quick@${WG_IFACE}"
fi
ok "WireGuard interface ${WG_IFACE} is up."

# ----------------------------- app system user + sudoers ---------------------
# The .NET backend runs as ${APP_USER} and needs to call wg / wg-quick.
log "Creating system user '${APP_USER}' ..."
if ! id -u "$APP_USER" >/dev/null 2>&1; then
  useradd --system --no-create-home --shell /usr/sbin/nologin "$APP_USER"
  ok "User '${APP_USER}' created."
else
  ok "User '${APP_USER}' already exists."
fi

log "Writing sudoers rule for '${APP_USER}' ..."
cat > "/etc/sudoers.d/iranconnect-wg" <<EOF
# Allow IranConnection backend to manage WireGuard peers without full root.
${APP_USER} ALL=(root) NOPASSWD: /usr/bin/wg, /usr/sbin/wg, /usr/bin/wg-quick, /usr/sbin/wg-quick
EOF
chmod 440 "/etc/sudoers.d/iranconnect-wg"
visudo -cf "/etc/sudoers.d/iranconnect-wg" || die "sudoers syntax error — check /etc/sudoers.d/iranconnect-wg"
ok "Sudoers rule written."

# ----------------------------- write backend config --------------------------
# The .NET backend reads /etc/iranconnect/appsettings.Production.json at startup
# (loaded in Program.cs via AddJsonFile). This auto-injects the server public
# key and endpoint so appsettings.json placeholder values are overridden.
log "Writing backend config to ${BACKEND_CFG_DIR}/appsettings.Production.json ..."
mkdir -p "$BACKEND_CFG_DIR"
chmod 750 "$BACKEND_CFG_DIR"
chown "root:${APP_USER}" "$BACKEND_CFG_DIR"

cat > "${BACKEND_CFG_DIR}/appsettings.Production.json" <<EOF
{
  "WireGuard": {
    "ServerPublicKey": "${SERVER_PUB}",
    "ServerEndpoint": "${PUBLIC_IP}:${WG_PORT}",
    "Dns": "${WG_DNS}",
    "Interface": "${WG_IFACE}",
    "Subnet": "${WG_NET}",
    "ClientAllowedIPs": "0.0.0.0/0, ::/0"
  }
}
EOF
chmod 640 "${BACKEND_CFG_DIR}/appsettings.Production.json"
chown "root:${APP_USER}" "${BACKEND_CFG_DIR}/appsettings.Production.json"
ok "Backend config written -> ${BACKEND_CFG_DIR}/appsettings.Production.json"

# ----------------------------- summary ---------------------------------------
echo
ok "================= DONE ================="
echo "  WireGuard endpoint : ${PUBLIC_IP}:${WG_PORT}"
echo "  Server public key  : ${SERVER_PUB}"
echo "  Tunnel subnet      : ${WG_NET} (server: ${WG_SERVER_IP})"
echo "  Client AllowedIPs  : 0.0.0.0/0, ::/0 (full tunnel)"
echo "  Split tunneling    : per-app, enforced by Android app"
echo "  App system user    : ${APP_USER}"
echo "  Backend config     : ${BACKEND_CFG_DIR}/appsettings.Production.json"
echo
echo "  Next steps:"
echo "    1. Build .NET backend: dotnet publish -c Release"
echo "    2. Run as '${APP_USER}': chown -R ${APP_USER} <publish-dir>"
echo "       systemctl start iranconnect   (or dotnet IranConnect.API.dll)"
echo "    3. Verify tunnel: wg show ${WG_IFACE}"
echo "    4. Service logs : journalctl -u wg-quick@${WG_IFACE} -e"
echo
ok "========================================"
