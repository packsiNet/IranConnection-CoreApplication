using IranConnect.Domain.Common;

namespace IranConnect.Domain.Entities;

public class WireGuardPeer : BaseEntity
{
    public Guid UserId { get; private set; }
    public string PublicKey { get; private set; } = default!;
    public string PrivateKey { get; private set; } = default!;
    public string AssignedIp { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;
    public DateTime? LastHandshake { get; private set; }
    public long BytesReceived { get; private set; }
    public long BytesSent { get; private set; }
    public DateTime? LastSeenAt { get; private set; }

    private WireGuardPeer() { }

    public static WireGuardPeer Create(
        Guid userId,
        string publicKey,
        string privateKey,
        string assignedIp)
    {
        return new WireGuardPeer
        {
            UserId = userId,
            PublicKey = publicKey,
            PrivateKey = privateKey,
            AssignedIp = assignedIp
        };
    }

    public void UpdateStats(
        long bytesReceived,
        long bytesSent,
        DateTime? lastHandshake)
    {
        BytesReceived = bytesReceived;
        BytesSent = bytesSent;
        LastHandshake = lastHandshake;
        LastSeenAt = DateTime.UtcNow;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;

    public bool IsOnline =>
        LastHandshake.HasValue &&
        DateTime.UtcNow - LastHandshake.Value < TimeSpan.FromMinutes(3);

    public User User { get; private set; } = default!;
}
