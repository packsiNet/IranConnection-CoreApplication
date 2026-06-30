using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Stats.Queries.GetDownloadStats;

public record GetDownloadStatsQuery(
    DateTime? From = null,
    DateTime? To = null
) : IRequest<Result<DownloadStatsResponse>>;

public record DownloadStatsResponse(
    int TotalDownloadClicks,
    int TotalLogins,
    List<AppDownloadStat> TopApps
);

public record AppDownloadStat(string PackageName, int ClickCount);
