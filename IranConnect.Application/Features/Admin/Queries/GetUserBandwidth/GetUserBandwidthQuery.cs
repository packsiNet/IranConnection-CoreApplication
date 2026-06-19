using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Admin.Queries.GetUserBandwidth;

public record GetUserBandwidthQuery(Guid TargetUserId)
    : IRequest<Result<UserBandwidthResponse>>;

public record UserBandwidthResponse(
    string UserId,
    string Email,
    long BytesReceived,
    long BytesSent,
    long TotalBytes,
    string BytesReceivedHuman,
    string BytesSentHuman,
    string TotalBytesHuman,
    long? LimitBytes,
    string? LimitHuman,
    double? UsagePercent,
    bool HasExceededLimit);
