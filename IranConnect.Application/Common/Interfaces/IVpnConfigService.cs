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
    string Dns);
