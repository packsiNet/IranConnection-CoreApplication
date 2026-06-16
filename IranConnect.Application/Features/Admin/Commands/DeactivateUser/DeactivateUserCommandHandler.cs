using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Admin.Commands.DeactivateUser;

public class DeactivateUserCommandHandler
    : IRequestHandler<DeactivateUserCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public DeactivateUserCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<string>> Handle(
        DeactivateUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(
                u => u.Id == request.TargetUserId,
                cancellationToken);

        if (user is null)
            return Result<string>.Failure("کاربر یافت نشد", 404);

        user.Deactivate();

        foreach (var token in user.RefreshTokens.Where(t => t.IsActive))
            token.Revoke("account deactivated");

        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success("حساب کاربری غیرفعال شد");
    }
}
