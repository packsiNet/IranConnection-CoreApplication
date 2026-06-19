namespace IranConnect.Application.Common.Interfaces;

public interface IWireGuardService
{
    Task<(string publicKey, string privateKey)> GenerateKeyPairAsync();
    Task AddPeerAsync(string publicKey, string assignedIp);
    Task RemovePeerAsync(string publicKey);
    Task<List<WireGuardPeerStats>> GetAllPeerStatsAsync();
}

public record WireGuardPeerStats(
    string PublicKey,
    long BytesReceived,
    long BytesSent,
    DateTime? LastHandshake,
    string Endpoint);
