namespace IranConnect.Application.Common.Interfaces;

public interface IWireGuardService
{
    Task<(string publicKey, string privateKey)> GenerateKeyPairAsync();
    Task AddPeerAsync(string publicKey, string assignedIp);
    Task RemovePeerAsync(string publicKey);
    Task<List<WireGuardPeerStats>> GetAllPeerStatsAsync();

    // Re-apply a full set of peers onto the live wg interface (idempotent).
    // Used at startup to resync wg0 with the DB after a service / wg-quick
    // restart wipes runtime peers (peers are managed dynamically, not stored
    // statically in wg0.conf).
    Task SyncPeersAsync(IReadOnlyCollection<WireGuardPeerConfig> peers);
}

public record WireGuardPeerConfig(string PublicKey, string AssignedIp);

public record WireGuardPeerStats(
    string PublicKey,
    long BytesReceived,
    long BytesSent,
    DateTime? LastHandshake,
    string Endpoint);
