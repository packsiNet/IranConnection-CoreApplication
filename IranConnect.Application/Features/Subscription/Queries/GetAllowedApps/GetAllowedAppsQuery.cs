using IranConnect.Application.Common.Models;
using IranConnect.Application.Features.Subscription.Queries.GetSubscription;
using MediatR;

namespace IranConnect.Application.Features.Subscription.Queries.GetAllowedApps;

public record GetAllowedAppsQuery(Guid UserId)
    : IRequest<Result<List<AllowedAppResponse>>>;
