using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Subscription.Queries.GetSubscription;

public record GetSubscriptionQuery(Guid UserId)
    : IRequest<Result<SubscriptionResponse>>;

public record SubscriptionResponse(
    string Plan,
    string Status,
    DateTime StartDate,
    DateTime ExpireDate,
    int DaysRemaining,
    bool IsActive,
    bool ShowAds,
    List<AllowedAppResponse> AllowedApps);

public record AllowedAppResponse(
    string PackageName,
    string NameEn,
    string NameFa);
