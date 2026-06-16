using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Subscription.Commands.DowngradeSubscription;

public record DowngradeSubscriptionCommand(Guid UserId)
    : IRequest<Result<string>>;
