using IranConnect.Application.Common.Models;
using IranConnect.Application.Features.Admin.Queries.GetUsers;
using MediatR;

namespace IranConnect.Application.Features.Admin.Queries.GetBandwidthReport;

public record GetBandwidthReportQuery(
    int Page = 1,
    int PageSize = 20,
    string? SortBy = "total")
    : IRequest<Result<PagedResult<BandwidthReportItem>>>;

public record BandwidthReportItem(
    string UserId,
    string Email,
    string AssignedIp,
    bool IsOnline,
    long BytesReceived,
    long BytesSent,
    long TotalBytes,
    string BytesReceivedHuman,
    string BytesSentHuman,
    string TotalBytesHuman,
    long? LimitBytes,
    string? LimitHuman,
    bool HasExceededLimit,
    DateTime? LastSeenAt);
