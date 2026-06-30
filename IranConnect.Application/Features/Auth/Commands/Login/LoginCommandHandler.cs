using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DomainRefreshToken = IranConnect.Domain.Entities.RefreshToken;

namespace IranConnect.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler
    : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public LoginCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtService jwtService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<Result<LoginResponse>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Subscription)
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(
                u => u.Email == request.Email.ToLowerInvariant().Trim(),
                cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            return Result<LoginResponse>.Failure("ایمیل یا پسورد اشتباه است", 401);

        if (!user.IsActive)
            return Result<LoginResponse>.Failure("حساب کاربری غیرفعال است", 403);

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

        var plan = user.Subscription?.Plan.ToString() ?? "Free";
        var showAds = user.Subscription?.ShowAds ?? true;
        var accessToken = _jwtService.GenerateToken(
            user.Id.ToString(), user.Email, plan, user.IsAdmin, showAds);

        return Result<LoginResponse>.Success(new LoginResponse(
            accessToken,
            refreshToken.Token,
            DateTime.UtcNow.AddMinutes(60),
            user.Email,
            user.FullName,
            plan,
            showAds,
            user.IsEmailVerified));
    }
}
