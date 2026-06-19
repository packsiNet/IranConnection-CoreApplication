using System.Security.Cryptography;
using System.Text;
using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using IranConnect.Application.Features.Auth.Commands.Login;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DomainRefreshToken = IranConnect.Domain.Entities.RefreshToken;
using DomainUser = IranConnect.Domain.Entities.User;
using DomainSubscription = IranConnect.Domain.Entities.Subscription;

namespace IranConnect.Application.Features.Auth.Commands.DeviceLogin;

public class DeviceLoginCommandHandler
    : IRequestHandler<DeviceLoginCommand, Result<LoginResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IVpnConfigService _vpnConfigService;
    private readonly ILogger<DeviceLoginCommandHandler> _logger;

    public DeviceLoginCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        IVpnConfigService vpnConfigService,
        ILogger<DeviceLoginCommandHandler> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _vpnConfigService = vpnConfigService;
        _logger = logger;
    }

    public async Task<Result<LoginResponse>> Handle(
        DeviceLoginCommand request,
        CancellationToken cancellationToken)
    {
        // DeviceId hashed before storing for privacy
        var hashedDeviceId = HashDeviceId(request.DeviceId);

        var existingUser = await _context.Users
            .Include(u => u.Subscription)
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(
                u => u.DeviceId == hashedDeviceId,
                cancellationToken);

        DomainUser user;
        var isNewUser = existingUser is null;

        if (existingUser is null)
        {
            // Register new device user. No client password: the device account is
            // created silently on first app open. A random password is generated and
            // hashed so a credential exists for any future email/password flow.
            var passwordHash = _passwordHasher.Hash(GenerateRandomPassword());
            user = DomainUser.CreateDeviceUser(hashedDeviceId, passwordHash);
            var subscription = DomainSubscription.CreateFree(user.Id);
            user.AttachSubscription(subscription);

            _context.Users.Add(user);
            _context.Subscriptions.Add(subscription);
        }
        else
        {
            // Login existing device user. The hashed DeviceId is the credential, so
            // no password check here — only the active-account guard.
            if (!existingUser.IsActive)
                return Result<LoginResponse>.Failure(
                    "حساب کاربری غیرفعال است", 403);

            user = existingUser;
        }

        // Revoke old tokens for same device
        var oldTokens = user.RefreshTokens
            .Where(t => t.IsActive && t.DeviceInfo == request.DeviceInfo)
            .ToList();
        foreach (var old in oldTokens)
            old.Revoke("new login from same device");

        var refreshToken = DomainRefreshToken.Create(
            user.Id,
            request.DeviceInfo,
            request.IpAddress);

        user.UpdateLastLogin();
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Auto-provision WireGuard peer for newly registered device users.
        // Guarded so provisioning failure (e.g. wg unavailable) does not block login.
        if (isNewUser)
        {
            try
            {
                await _vpnConfigService.GetOrCreatePeerAsync(user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to auto-provision WireGuard peer for user {UserId}",
                    user.Id);
            }
        }

        var plan = user.Subscription?.Plan.ToString() ?? "Free";
        var accessToken = _jwtService.GenerateToken(
            user.Id.ToString(),
            user.Email,
            plan,
            user.IsAdmin);

        return Result<LoginResponse>.Success(new LoginResponse(
            accessToken,
            refreshToken.Token,
            DateTime.UtcNow.AddMinutes(60),
            user.Email,
            user.FullName,
            plan,
            user.IsEmailVerified));
    }

    private static string HashDeviceId(string deviceId)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(deviceId));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    // Server-side random password for silently-created device accounts.
    // 32 bytes of CSPRNG entropy, base64-encoded.
    private static string GenerateRandomPassword()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
}
