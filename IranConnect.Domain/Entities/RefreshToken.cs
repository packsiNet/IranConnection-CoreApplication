using System.Security.Cryptography;
using IranConnect.Domain.Common;

namespace IranConnect.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = default!;
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public string? RevokedReason { get; private set; }
    public string? DeviceInfo { get; private set; }
    public string? IpAddress { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;

    public User User { get; private set; } = default!;

    private RefreshToken() { }

    public static RefreshToken Create(
        Guid userId,
        string? deviceInfo = null,
        string? ipAddress = null)
    {
        return new RefreshToken
        {
            UserId = userId,
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
                .Replace("+", "-").Replace("/", "_").Replace("=", ""),
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            DeviceInfo = deviceInfo,
            IpAddress = ipAddress
        };
    }

    public void Revoke(string reason)
    {
        IsRevoked = true;
        RevokedReason = reason;
        RevokedAt = DateTime.UtcNow;
    }
}
