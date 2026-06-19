using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Admin.Commands.ActivateUser;

public class ActivateUserCommandHandler
    : IRequestHandler<ActivateUserCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public ActivateUserCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<string>> Handle(
        ActivateUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(
                u => u.Id == request.TargetUserId,
                cancellationToken);

        if (user is null)
            return Result<string>.Failure("User not found", 404);

        if (user.IsActive)
            return Result<string>.Failure("Account is already active", 400);

        user.Activate();
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success("Account activated");
    }
}
