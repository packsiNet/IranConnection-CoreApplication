using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.User.Queries.GetProfile;

public class GetProfileQueryHandler
    : IRequestHandler<GetProfileQuery, Result<ProfileResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetProfileQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<ProfileResponse>> Handle(
        GetProfileQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Subscription)
            .FirstOrDefaultAsync(
                u => u.Id == request.UserId,
                cancellationToken);

        if (user is null)
            return Result<ProfileResponse>.Failure("کاربر یافت نشد", 404);

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
                user.Subscription?.IsActive ?? false)));
    }
}
