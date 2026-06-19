using System.Security.Cryptography;
using IranConnect.Domain.Common;

namespace IranConnect.Domain.Entities;

public class User : AuditableEntity
{
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string? FullName { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool IsAdmin { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public string? EmailVerificationToken { get; private set; }
    public DateTime? EmailVerificationTokenExpiry { get; private set; }
    public string? PasswordResetToken { get; private set; }
    public DateTime? PasswordResetTokenExpiry { get; private set; }
    public string? DeviceId { get; private set; }
    public bool IsDeviceUser { get; private set; }

    public ICollection<RefreshToken> RefreshTokens { get; private set; }
        = new List<RefreshToken>();
    public Subscription? Subscription { get; private set; }

    private User() { }

    public static User Create(string email, string passwordHash, string? fullName = null)
    {
        return new User
        {
            Email = email.ToLowerInvariant().Trim(),
            PasswordHash = passwordHash,
            FullName = fullName,
            EmailVerificationToken = GenerateCode(),
            EmailVerificationTokenExpiry = DateTime.UtcNow.AddMinutes(15)
        };
    }

    public static User CreateDeviceUser(string deviceId, string passwordHash)
    {
        return new User
        {
            Email = $"device_{deviceId}@iranconnect.internal",
            PasswordHash = passwordHash,
            IsEmailVerified = true,
            IsActive = true,
            IsDeviceUser = true,
            DeviceId = deviceId
        };
    }

    public void UpdateDevicePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
    }

    public void VerifyEmail()
    {
        IsEmailVerified = true;
        EmailVerificationToken = null;
        EmailVerificationTokenExpiry = null;
    }

    public void SetPasswordResetToken()
    {
        PasswordResetToken = GenerateCode();
        PasswordResetTokenExpiry = DateTime.UtcNow.AddMinutes(15);
    }

    public void ResetPassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        PasswordResetToken = null;
        PasswordResetTokenExpiry = null;
    }

    public void UpdateLastLogin()
        => LastLoginAt = DateTime.UtcNow;

    public void Deactivate()
        => IsActive = false;

    public void Activate()
        => IsActive = true;

    public void AttachSubscription(Subscription subscription)
        => Subscription = subscription;

    public void UpdateFullName(string fullName)
        => FullName = fullName.Trim();

    public void RegenerateVerificationToken()
    {
        EmailVerificationToken = GenerateCode();
        EmailVerificationTokenExpiry = DateTime.UtcNow.AddMinutes(15);
    }

    public bool IsEmailVerificationTokenValid(string code)
        => EmailVerificationToken == code &&
           EmailVerificationTokenExpiry > DateTime.UtcNow;

    public bool IsPasswordResetTokenValid(string code)
        => PasswordResetToken == code &&
           PasswordResetTokenExpiry > DateTime.UtcNow;

    // 6-digit numeric code, sent by email and verified in-app
    private static string GenerateCode()
        => RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
}
