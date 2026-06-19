using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Admin.Queries.GetOnlineStats;

public record GetOnlineStatsQuery
    : IRequest<Result<OnlineStatsResponse>>;

public record OnlineStatsResponse(
    int TotalPeers,
    int OnlinePeers,
    long TotalBytesReceived,
    long TotalBytesSent,
    string TotalBytesReceivedHuman,
    string TotalBytesSentHuman);
