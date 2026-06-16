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
    private readonly IIranianAppService _iranianAppService;

    public UpgradeSubscriptionCommandHandler(
        IApplicationDbContext context,
        IIranianAppService iranianAppService)
    {
        _context = context;
        _iranianAppService = iranianAppService;
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

        var allowedApps = _iranianAppService
            .GetAllowedApps(subscription.Plan);

        return Result<SubscriptionResponse>.Success(
            new SubscriptionResponse(
                subscription.Plan.ToString(),
                subscription.Status.ToString(),
                subscription.StartDate,
                subscription.ExpireDate,
                subscription.DaysRemaining,
                subscription.IsActive,
                allowedApps.Select(a => new AllowedAppResponse(
                    a.PackageName,
                    a.NameEn,
                    a.NameFa)).ToList()));
    }
}
