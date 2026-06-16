using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using IranConnect.Application.Features.Auth.Commands.Login;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DomainRefreshToken = IranConnect.Domain.Entities.RefreshToken;

namespace IranConnect.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler
    : IRequestHandler<RefreshTokenCommand, Result<LoginResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;

    public RefreshTokenCommandHandler(
        IApplicationDbContext context,
        IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<Result<LoginResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var refreshToken = await _context.RefreshTokens
            .Include(r => r.User)
                .ThenInclude(u => u.Subscription)
            .FirstOrDefaultAsync(
                r => r.Token == request.Token,
                cancellationToken);

        if (refreshToken is null || !refreshToken.IsActive)
            return Result<LoginResponse>.Failure("توکن نامعتبر است", 401);

        refreshToken.Revoke("refreshed");

        var newRefreshToken = DomainRefreshToken.Create(
            refreshToken.UserId,
            request.DeviceInfo,
            request.IpAddress);

        refreshToken.User.UpdateLastLogin();
        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        var plan = refreshToken.User.Subscription?.Plan.ToString() ?? "Free";
        var accessToken = _jwtService.GenerateToken(
            refreshToken.User.Id.ToString(),
            refreshToken.User.Email,
            plan,
            refreshToken.User.IsAdmin);

        return Result<LoginResponse>.Success(new LoginResponse(
            accessToken,
            newRefreshToken.Token,
            DateTime.UtcNow.AddMinutes(60),
            refreshToken.User.Email,
            refreshToken.User.FullName,
            plan,
            refreshToken.User.IsEmailVerified));
    }
}
