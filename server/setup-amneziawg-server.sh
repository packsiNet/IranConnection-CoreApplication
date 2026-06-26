#!/usr/bin/env bash
#
# setup-amneziawg-server.sh
# -----------------------------------------------------------------------------
# AmneziaWG (awg) server bootstrap for the IranConnection backend.
#
# Why AmneziaWG instead of plain WireGuard:
#   Raw WireGuard has a fixed packet fingerprint (148-byte handshake init,
#   92-byte response, fixed message-type headers). Iranian carrier (MCI) and
#   border DPI drop these, so clients on mobile / outside Iran never complete a
#   handshake (wg show => latest handshake = 0). AmneziaWG randomizes headers
#   and injects junk packets (Jc/Jmin/Jmax/S1/S2/H1-H4), defeating the DPI.
#
# What it does (idempotent):
#   1. Verifies root + Debian/Ubuntu.
#   2. Installs AmneziaWG kernel module (DKMS) + tools (provides awg/awg-quick).
#   3. Generates server key pair with awg (reused if present).
#   4. Generates + persists obfuscation params (reused if present so existing
#      clients keep working).
#   5. Detects public IP + WAN interface, enables forwarding + NAT.
#   6. Writes /etc/amnezia/amneziawg/wg0.conf ([Interface] only, no static Peer;
#      backend manages peers dynamically via `awg set`).
#   7. Opens UDP port + SSH on UFW.
#   8. Creates system user + sudoers for awg / awg-quick.
#   9. Writes /etc/iranconnect/appsettings.Production.json including the same
#      obfuscation params so the backend serves identical values to clients.
#
# Usage:   sudo bash setup-amneziawg-server.sh
#
# Env overrides (optional):
#   WG_PORT (443)  WG_NET (10.0.0.0/24)  WG_DNS (1.1.1.1)
#   PUBLIC_IP (auto)  SSH_PORT (22)  APP_USER (iranconnect)
# -----------------------------------------------------------------------------

set -Eeuo pipefail

# ----------------------------- configuration ---------------------------------
WG_IFACE="wg0"
WG_PORT="${WG_PORT:-443}"
# Subnet MUST match VpnConfigService.cs IP allocation (10.0.0.2 … .254)
WG_NET="${WG_NET:-10.0.0.0/24}"
WG_DNS="${WG_DNS:-1.1.1.1}"
# AmneziaWG keeps configs under /etc/amnezia/amneziawg (awg-quick default).
WG_DIR="/etc/amnezia/amneziawg"
SSH_PORT="${SSH_PORT:-22}"
APP_USER="${APP_USER:-iranconnect}"
PUBLIC_IP="${PUBLIC_IP:-95.38.163.117}"
BACKEND_CFG_DIR="/etc/iranconnect"
OBFS_FILE="${WG_DIR}/obfuscation.env"

WG_BASE="${WG_NET%.*}"
WG_SERVER_IP="${WG_BASE}.1"
WG_NET_PREFIX="${WG_NET##*/}"

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

# ----------------------------- pre-flight ------------------------------------
[[ "${EUID}" -eq 0 ]] || die "Run as root:  sudo bash $0"
[[ -r /etc/os-release ]] || die "Cannot read /etc/os-release."
# shellcheck disable=SC1091
. /etc/os-release
log "Detected OS: ${PRETTY_NAME:-$ID}"
case "${ID:-}${ID_LIKE:-}" in
  *debian*|*ubuntu*) : ;;
  *) die "This AmneziaWG script supports Debian/Ubuntu only. For RHEL build awg from source: https://github.com/amnezia-vpn/amneziawg-linux-kernel-module" ;;
esac

# ----------------------------- install AmneziaWG -----------------------------
install_amneziawg() {
  if command -v awg >/dev/null && command -v awg-quick >/dev/null; then
    ok "AmneziaWG already installed."
    return
  fi
  log "Installing AmneziaWG (kernel module + tools) ..."
  export DEBIAN_FRONTEND=noninteractive
  apt-get update -y
  apt-get install -y software-properties-common dkms \
    linux-headers-"$(uname -r)" ufw iptables curl jq iproute2 || true

  # Official AmneziaWG PPA (Ubuntu). For Debian, the PPA also works via
  # add-apt-repository on most releases; falls back to source build below.
  if add-apt-repository -y ppa:amnezia/ppa 2>/dev/null; then
    apt-get update -y
    apt-get install -y amneziawg amneziawg-tools || true
  fi

  if ! command -v awg >/dev/null; then
    warn "PPA install unavailable; building AmneziaWG from source ..."
    build_amneziawg_from_source
  fi

  command -v awg       >/dev/null || die "awg not found after install."
  command -v awg-quick >/dev/null || die "awg-quick not found after install."
  ok "AmneziaWG installed."
}

build_amneziawg_from_source() {
  apt-get install -y git build-essential make
  local tmp; tmp="$(mktemp -d)"
  git clone https://github.com/amnezia-vpn/amneziawg-linux-kernel-module.git \
    "${tmp}/mod"
  git clone https://github.com/amnezia-vpn/amneziawg-tools.git "${tmp}/tools"
  make -C "${tmp}/mod/src" -j"$(nproc)"
  make -C "${tmp}/mod/src" install
  make -C "${tmp}/tools/src" -j"$(nproc)"
  make -C "${tmp}/tools/src" install
  depmod -a
  rm -rf "$tmp"
}
install_amneziawg

if ! modprobe amneziawg 2>/dev/null && [[ ! -d /sys/module/amneziawg ]]; then
  warn "amneziawg kernel module not loaded; check 'dmesg | grep amneziawg'."
fi

# ----------------------------- keys ------------------------------------------
umask 077
mkdir -p "$WG_DIR"

if [[ -s "${WG_DIR}/server_private" && -s "${WG_DIR}/server_public" ]]; then
  log "Re-using existing server keys."
else
  log "Generating server key pair ..."
  awg genkey | tee "${WG_DIR}/server_private" | awg pubkey \
    > "${WG_DIR}/server_public"
fi
chmod 600 "${WG_DIR}/server_private" "${WG_DIR}/server_public"
SERVER_PRIV="$(cat "${WG_DIR}/server_private")"
SERVER_PUB="$(cat "${WG_DIR}/server_public")"

# ----------------------------- obfuscation params ----------------------------
# Generate once and persist. Changing these later breaks every existing client,
# so we reuse the saved file on re-runs.
rand_u32() { od -An -N4 -tu4 /dev/urandom | tr -d '[:space:]'; }

if [[ -s "$OBFS_FILE" ]]; then
  log "Re-using existing obfuscation params (${OBFS_FILE})."
  # shellcheck disable=SC1090
  . "$OBFS_FILE"
else
  log "Generating obfuscation params ..."
  JC=8; JMIN=24; JMAX=80; S1=24; S2=48
  # H1-H4 must be distinct uint32 > 4. Regenerate on rare collision.
  H1="$(rand_u32)"; H2="$(rand_u32)"; H3="$(rand_u32)"; H4="$(rand_u32)"
  while [[ "$H1" == "$H2" || "$H1" == "$H3" || "$H1" == "$H4" || \
           "$H2" == "$H3" || "$H2" == "$H4" || "$H3" == "$H4" ]]; do
    H2="$(rand_u32)"; H3="$(rand_u32)"; H4="$(rand_u32)"
  done
  cat > "$OBFS_FILE" <<EOF
JC=${JC}
JMIN=${JMIN}
JMAX=${JMAX}
S1=${S1}
S2=${S2}
H1=${H1}
H2=${H2}
H3=${H3}
H4=${H4}
EOF
  chmod 600 "$OBFS_FILE"
fi
ok "Obfuscation: Jc=${JC} Jmin=${JMIN} Jmax=${JMAX} S1=${S1} S2=${S2} H1..H4 set."

# ----------------------------- network detection -----------------------------
detect_public_ip() {
  if [[ -n "${PUBLIC_IP:-}" ]]; then echo "$PUBLIC_IP"; return; fi
  local ip=""
  for url in "https://api.ipify.org" "https://ifconfig.me/ip" "https://ipinfo.io/ip"; do
    ip="$(curl -fsS --max-time 8 "$url" 2>/dev/null | tr -d '[:space:]')" || true
    [[ "$ip" =~ ^[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+$ ]] && { echo "$ip"; return; }
  done
  die "Could not auto-detect public IP. Re-run with PUBLIC_IP=<your.ip>."
}
PUBLIC_IP="$(detect_public_ip)"
ok "Public IP: ${PUBLIC_IP}"

WAN_IFACE="$(ip -4 route show default | awk '/default/ {print $5; exit}')"
[[ -n "$WAN_IFACE" ]] || die "Cannot determine default network interface."
ok "WAN interface: ${WAN_IFACE}"

# ----------------------------- IP forwarding ---------------------------------
log "Enabling IP forwarding ..."
cat > /etc/sysctl.d/99-amneziawg-forward.conf <<EOF
net.ipv4.ip_forward = 1
net.ipv6.conf.all.forwarding = 1
EOF
sysctl --system >/dev/null
ok "IP forwarding enabled."

# ----------------------------- wg0.conf --------------------------------------
# [Interface] only — backend adds peers dynamically: awg set wg0 peer <pk> ...
# Obfuscation params live here AND in the backend config; they MUST match.
log "Writing ${WG_DIR}/${WG_IFACE}.conf ..."
cat > "${WG_DIR}/${WG_IFACE}.conf" <<EOF
# Managed by setup-amneziawg-server.sh + IranConnection backend.
# Do NOT add static [Peer] blocks — the backend manages peers dynamically.
[Interface]
Address    = ${WG_SERVER_IP}/${WG_NET_PREFIX}
ListenPort = ${WG_PORT}
PrivateKey = ${SERVER_PRIV}

# AmneziaWG DPI-evasion (must match every client's [Interface]).
Jc   = ${JC}
Jmin = ${JMIN}
Jmax = ${JMAX}
S1   = ${S1}
S2   = ${S2}
H1   = ${H1}
H2   = ${H2}
H3   = ${H3}
H4   = ${H4}

PostUp   = iptables -A FORWARD -i %i -j ACCEPT; \\
           iptables -A FORWARD -o %i -j ACCEPT; \\
           iptables -t nat -A POSTROUTING -o ${WAN_IFACE} -j MASQUERADE
PostDown = iptables -D FORWARD -i %i -j ACCEPT; \\
           iptables -D FORWARD -o %i -j ACCEPT; \\
           iptables -t nat -D POSTROUTING -o ${WAN_IFACE} -j MASQUERADE
EOF
chmod 600 "${WG_DIR}/${WG_IFACE}.conf"
ok "wg0.conf written (Interface + obfuscation, no static peers)."

# ----------------------------- UFW firewall ----------------------------------
log "Configuring UFW ..."
ufw allow "${SSH_PORT}/tcp" comment 'SSH' >/dev/null || true
ufw allow "${WG_PORT}/udp" comment 'AmneziaWG' >/dev/null
if ! grep -q '^DEFAULT_FORWARD_POLICY="ACCEPT"' /etc/default/ufw 2>/dev/null; then
  sed -i 's/^DEFAULT_FORWARD_POLICY=.*/DEFAULT_FORWARD_POLICY="ACCEPT"/' /etc/default/ufw
fi
if ufw status | grep -q "Status: inactive"; then
  ufw --force enable >/dev/null
else
  ufw reload >/dev/null || true
fi
ok "UFW: SSH ${SSH_PORT}/tcp + AmneziaWG ${WG_PORT}/udp open; forwarding ACCEPT."

# ----------------------------- start service ---------------------------------
log "Starting AmneziaWG service ..."
# Stop any plain-WireGuard instance on the same iface to avoid port clash.
systemctl disable --now "wg-quick@${WG_IFACE}" >/dev/null 2>&1 || true
systemctl enable "awg-quick@${WG_IFACE}" >/dev/null 2>&1 || true
systemctl restart "awg-quick@${WG_IFACE}"
sleep 1
if ! awg show "${WG_IFACE}" >/dev/null 2>&1; then
  die "awg-quick@${WG_IFACE} failed. Check: journalctl -u awg-quick@${WG_IFACE} -e"
fi
ok "AmneziaWG interface ${WG_IFACE} is up (listening on ${WG_PORT})."

# ----------------------------- app user + sudoers ----------------------------
log "Creating system user '${APP_USER}' ..."
if ! id -u "$APP_USER" >/dev/null 2>&1; then
  useradd --system --no-create-home --shell /usr/sbin/nologin "$APP_USER"
  ok "User '${APP_USER}' created."
else
  ok "User '${APP_USER}' already exists."
fi

log "Writing sudoers rule for '${APP_USER}' ..."
cat > "/etc/sudoers.d/iranconnect-wg" <<EOF
# Allow IranConnection backend to manage AmneziaWG peers without full root.
${APP_USER} ALL=(root) NOPASSWD: /usr/bin/awg, /usr/sbin/awg, /usr/bin/awg-quick, /usr/sbin/awg-quick
EOF
chmod 440 "/etc/sudoers.d/iranconnect-wg"
visudo -cf "/etc/sudoers.d/iranconnect-wg" \
  || die "sudoers syntax error — check /etc/sudoers.d/iranconnect-wg"
ok "Sudoers rule written."

# ----------------------------- backend config --------------------------------
log "Writing backend config -> ${BACKEND_CFG_DIR}/appsettings.Production.json ..."
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
    "ClientAllowedIPs": "0.0.0.0/0, ::/0",
    "Binary": "awg",
    "QuickBinary": "awg-quick",
    "Obfuscation": {
      "Jc": ${JC},
      "Jmin": ${JMIN},
      "Jmax": ${JMAX},
      "S1": ${S1},
      "S2": ${S2},
      "H1": ${H1},
      "H2": ${H2},
      "H3": ${H3},
      "H4": ${H4}
    }
  }
}
EOF
chmod 640 "${BACKEND_CFG_DIR}/appsettings.Production.json"
chown "root:${APP_USER}" "${BACKEND_CFG_DIR}/appsettings.Production.json"
ok "Backend config written."

# ----------------------------- summary ---------------------------------------
echo
ok "================= DONE (AmneziaWG) ================="
echo "  Endpoint        : ${PUBLIC_IP}:${WG_PORT} (UDP, awg)"
echo "  Server pubkey   : ${SERVER_PUB}"
echo "  Subnet          : ${WG_NET} (server ${WG_SERVER_IP})"
echo "  Obfuscation     : Jc=${JC} Jmin=${JMIN} Jmax=${JMAX} S1=${S1} S2=${S2}"
echo "                    H1=${H1} H2=${H2} H3=${H3} H4=${H4}"
echo "  App user        : ${APP_USER}"
echo "  Backend config  : ${BACKEND_CFG_DIR}/appsettings.Production.json"
echo
echo "  Next steps:"
echo "    1. Restart backend so it loads awg binary + obfuscation params:"
echo "         systemctl restart iranconnect"
echo "    2. Backend startup sync re-applies DB peers onto awg wg0."
echo "    3. Verify:  awg show ${WG_IFACE}    (peers + listening port ${WG_PORT})"
echo "    4. Client MUST be AmneziaWG-capable and use identical H1-H4/S1/S2."
echo
warn "IMPORTANT: the Android client must use the AmneziaWG backend and apply"
warn "the SAME Jc/Jmin/Jmax/S1/S2/H1-H4 (served via GET /vpn/config). Plain"
warn "WireGuard clients will NOT connect to this interface."
ok "===================================================="
