using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Admin.Queries.GetPeerDetail;

public record GetPeerDetailQuery(Guid TargetUserId)
    : IRequest<Result<PeerDetailResponse>>;

public record PeerDetailResponse(
    string UserId,
    string UserEmail,
    string AssignedIp,
    string PublicKey,
    bool IsOnline,
    bool IsActive,
    long BytesReceived,
    long BytesSent,
    string BytesReceivedHuman,
    string BytesSentHuman,
    DateTime? LastHandshake,
    DateTime? LastSeenAt,
    DateTime CreatedAt,
    long? BandwidthLimitBytes,
    string? BandwidthLimitHuman);
