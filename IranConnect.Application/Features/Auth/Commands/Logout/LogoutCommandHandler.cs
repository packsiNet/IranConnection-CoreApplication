using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler
    : IRequestHandler<LogoutCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public LogoutCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<string>> Handle(
        LogoutCommand request,
        CancellationToken cancellationToken)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(
                t => t.Token == request.RefreshToken,
                cancellationToken);

        if (token is null || !token.IsActive)
            return Result<string>.Failure("توکن نامعتبر است", 401);

        token.Revoke("logout");
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success("خروج موفق");
    }
}
