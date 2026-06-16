using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.User.Queries.GetProfile;

public record GetProfileQuery(Guid UserId)
    : IRequest<Result<ProfileResponse>>;

public record ProfileResponse(
    string Id,
    string Email,
    string? FullName,
    bool IsEmailVerified,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    SubscriptionInfo Subscription);

public record SubscriptionInfo(
    string Plan,
    string Status,
    DateTime ExpireDate,
    int DaysRemaining,
    bool IsActive);
