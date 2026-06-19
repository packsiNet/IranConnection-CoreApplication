using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Admin.Queries.GetUserDetail;

public record GetUserDetailQuery(Guid TargetUserId)
    : IRequest<Result<UserDetailResponse>>;

public record UserDetailResponse(
    string Id,
    string Email,
    string? FullName,
    bool IsEmailVerified,
    bool IsActive,
    bool IsDeviceUser,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    SubscriptionDetail Subscription,
    WireGuardPeerDetail? Peer,
    List<ReceiptSummary> RecentReceipts);

public record SubscriptionDetail(
    string Plan,
    string Status,
    DateTime StartDate,
    DateTime ExpireDate,
    int DaysRemaining,
    bool IsActive);

public record WireGuardPeerDetail(
    string AssignedIp,
    string PublicKey,
    bool IsOnline,
    string BytesReceivedHuman,
    string BytesSentHuman,
    DateTime? LastHandshake,
    DateTime? LastSeenAt);

public record ReceiptSummary(
    string Id,
    string Status,
    int RequestedDurationDays,
    DateTime SubmittedAt,
    DateTime? ReviewedAt);
