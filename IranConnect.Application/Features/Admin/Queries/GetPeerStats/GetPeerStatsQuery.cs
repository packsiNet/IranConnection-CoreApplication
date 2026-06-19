using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Admin.Queries.GetPeerStats;

public record GetPeerStatsQuery
    : IRequest<Result<List<PeerStatsResponse>>>;

public record PeerStatsResponse(
    string UserId,
    string Email,
    string AssignedIp,
    string PublicKey,
    bool IsOnline,
    long BytesReceived,
    long BytesSent,
    string BytesReceivedHuman,
    string BytesSentHuman,
    DateTime? LastHandshake,
    DateTime? LastSeenAt);
