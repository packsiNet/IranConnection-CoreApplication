using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using IranConnect.Application.Features.Subscription.Queries.GetSubscription;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Subscription.Commands.UpgradeSubscription;

public class UpgradeSubscriptionCommandHandler
    : IRequestHandler<UpgradeSubscriptionCommand, Result<SubscriptionResponse>>
{
    private readonly IApplicationDbContext _context;

    public UpgradeSubscriptionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<SubscriptionResponse>> Handle(
        UpgradeSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(
                s => s.UserId == request.UserId,
                cancellationToken);

        if (subscription is null)
            return Result<SubscriptionResponse>.Failure(
                "اشتراک یافت نشد", 404);

        subscription.Upgrade(request.DurationDays);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<SubscriptionResponse>.Success(
            new SubscriptionResponse(
                subscription.Plan.ToString(),
                subscription.Status.ToString(),
                subscription.StartDate,
                subscription.ExpireDate,
                subscription.DaysRemaining,
                subscription.IsActive,
                subscription.ShowAds,
                allowedApps.Select(a => new AllowedAppResponse(
                    a.PackageName,
                    a.NameEn,
                    a.NameFa)).ToList()));
    }
}
