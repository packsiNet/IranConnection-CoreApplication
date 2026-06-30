using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using IranConnect.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Stats.Queries.GetDownloadStats;

public class GetDownloadStatsQueryHandler
    : IRequestHandler<GetDownloadStatsQuery, Result<DownloadStatsResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetDownloadStatsQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<DownloadStatsResponse>> Handle(
        GetDownloadStatsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.StatEvents.AsQueryable();

        if (request.From.HasValue)
            query = query.Where(e => e.CreatedAt >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(e => e.CreatedAt <= request.To.Value);

        var totalLogins = await query
            .CountAsync(e => e.EventType == StatEventType.Login, cancellationToken);

        var totalDownloads = await query
            .CountAsync(e => e.EventType == StatEventType.DownloadClick, cancellationToken);

        var topApps = await query
            .Where(e => e.EventType == StatEventType.DownloadClick && e.Metadata != null)
            .GroupBy(e => e.Metadata!)
            .Select(g => new AppDownloadStat(g.Key, g.Count()))
            .OrderByDescending(x => x.ClickCount)
            .Take(20)
            .ToListAsync(cancellationToken);

        return Result<DownloadStatsResponse>.Success(
            new DownloadStatsResponse(totalDownloads, totalLogins, topApps));
    }
}
