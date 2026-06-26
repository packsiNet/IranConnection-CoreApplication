namespace IranConnect.Application.Common.Interfaces;

public interface IVpnConfigService
{
    Task<WireGuardClientConfig> GetOrCreatePeerAsync(Guid userId);
}

public record WireGuardClientConfig(
    string PrivateKey,
    string AssignedIp,
    string ServerPublicKey,
    string ServerEndpoint,
    string Dns,
    // Traffic routed through tunnel. Split-tunneling is per-app on Android side.
    string AllowedIPs,
    // AmneziaWG DPI-evasion params. MUST match the server interface, else the
    // handshake is dropped on Iranian carrier/border paths (raw WG fingerprint).
    AmneziaObfuscation Obfuscation);

// AmneziaWG obfuscation parameters (awg). Jc/Jmin/Jmax = junk packet count/size
// range; S1/S2 = init/response junk header sizes; H1-H4 = randomized magic
// message-type headers. All clients share the interface-level values.
public record AmneziaObfuscation(
    int Jc,
    int Jmin,
    int Jmax,
    int S1,
    int S2,
    long H1,
    long H2,
    long H3,
    long H4);
