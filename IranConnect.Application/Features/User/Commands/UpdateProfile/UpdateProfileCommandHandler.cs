using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using IranConnect.Application.Features.User.Queries.GetProfile;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.User.Commands.UpdateProfile;

public class UpdateProfileCommandHandler
    : IRequestHandler<UpdateProfileCommand, Result<ProfileResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public UpdateProfileCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<ProfileResponse>> Handle(
        UpdateProfileCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Subscription)
            .FirstOrDefaultAsync(
                u => u.Id == request.UserId,
                cancellationToken);

        if (user is null)
            return Result<ProfileResponse>.Failure("کاربر یافت نشد", 404);

        if (request.FullName is not null)
            user.UpdateFullName(request.FullName);

        if (request.NewPassword is not null)
        {
            if (!_passwordHasher.Verify(request.CurrentPassword!, user.PasswordHash))
                return Result<ProfileResponse>.Failure("پسورد فعلی اشتباه است", 400);

            user.ResetPassword(_passwordHasher.Hash(request.NewPassword));
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<ProfileResponse>.Success(new ProfileResponse(
            user.Id.ToString(),
            user.Email,
            user.FullName,
            user.IsEmailVerified,
            user.CreatedAt,
            user.LastLoginAt,
            new SubscriptionInfo(
                user.Subscription?.Plan.ToString() ?? "Free",
                user.Subscription?.Status.ToString() ?? "Active",
                user.Subscription?.ExpireDate ?? DateTime.UtcNow.AddYears(100),
                user.Subscription?.DaysRemaining ?? 0,
                user.Subscription?.IsActive ?? false,
                user.Subscription?.ShowAds ?? true)));
    }
}
