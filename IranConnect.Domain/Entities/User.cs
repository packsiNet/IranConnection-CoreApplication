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
            EmailVerificationToken = GenerateSecureToken(),
            EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24)
        };
    }

    public void VerifyEmail()
    {
        IsEmailVerified = true;
        EmailVerificationToken = null;
        EmailVerificationTokenExpiry = null;
    }

    public void SetPasswordResetToken()
    {
        PasswordResetToken = GenerateSecureToken();
        PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
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

    public void AttachSubscription(Subscription subscription)
        => Subscription = subscription;

    public void UpdateFullName(string fullName)
        => FullName = fullName.Trim();

    public void RegenerateVerificationToken()
    {
        EmailVerificationToken = GenerateSecureToken();
        EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
    }

    public bool IsEmailVerificationTokenValid(string token)
        => EmailVerificationToken == token &&
           EmailVerificationTokenExpiry > DateTime.UtcNow;

    public bool IsPasswordResetTokenValid(string token)
        => PasswordResetToken == token &&
           PasswordResetTokenExpiry > DateTime.UtcNow;

    private static string GenerateSecureToken()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace("+", "-").Replace("/", "_").Replace("=", "");
}
