using IranConnect.Application.Common.Models;
using IranConnect.Application.Features.Subscription.Queries.GetSubscription;
using MediatR;

namespace IranConnect.Application.Features.Subscription.Commands.UpgradeSubscription;

public record UpgradeSubscriptionCommand(
    Guid UserId,
    int DurationDays = 30) : IRequest<Result<SubscriptionResponse>>;
