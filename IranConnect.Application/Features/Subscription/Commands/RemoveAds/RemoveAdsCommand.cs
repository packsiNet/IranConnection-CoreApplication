using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Subscription.Commands.RemoveAds;

public record RemoveAdsCommand(Guid UserId) : IRequest<Result<string>>;
