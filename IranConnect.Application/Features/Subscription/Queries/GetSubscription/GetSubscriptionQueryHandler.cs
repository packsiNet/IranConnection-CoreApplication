using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using IranConnect.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Subscription.Queries.GetSubscription;

public class GetSubscriptionQueryHandler
    : IRequestHandler<GetSubscriptionQuery, Result<SubscriptionResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetSubscriptionQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<SubscriptionResponse>> Handle(
        GetSubscriptionQuery request,
        CancellationToken cancellationToken)
    {
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(
                s => s.UserId == request.UserId,
                cancellationToken);

        if (subscription is null)
            return Result<SubscriptionResponse>.Failure(
                "اشتراک یافت نشد", 404);

        // Check if subscription is expired and update
        if (!subscription.IsActive &&
            subscription.Status == SubscriptionStatus.Active)
        {
            subscription.Expire();
            await _context.SaveChangesAsync(cancellationToken);
        }

        var isFreeplan = subscription.Plan == SubscriptionPlan.Free;
        var allowedApps = await _context.IranianApps
            .Where(a => a.IsActive && (!isFreeplan || a.IsFree))
            .OrderBy(a => a.NameEn)
            .Select(a => new AllowedAppResponse(a.PackageName, a.NameEn, a.NameFa))
            .ToListAsync(cancellationToken);

        return Result<SubscriptionResponse>.Success(
            new SubscriptionResponse(
                subscription.Plan.ToString(),
                subscription.Status.ToString(),
                subscription.StartDate,
                subscription.ExpireDate,
                subscription.DaysRemaining,
                subscription.IsActive,
                subscription.ShowAds,
                allowedApps));
    }
}
