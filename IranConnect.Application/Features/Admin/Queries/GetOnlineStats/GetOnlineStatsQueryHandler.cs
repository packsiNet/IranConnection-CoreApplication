using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Admin.Queries.GetOnlineStats;

public class GetOnlineStatsQueryHandler
    : IRequestHandler<GetOnlineStatsQuery, Result<OnlineStatsResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetOnlineStatsQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<OnlineStatsResponse>> Handle(
        GetOnlineStatsQuery request,
        CancellationToken cancellationToken)
    {
        var peers = await _context.WireGuardPeers
            .Where(p => p.IsActive)
            .ToListAsync(cancellationToken);

        var onlineThreshold = DateTime.UtcNow
            .Subtract(TimeSpan.FromMinutes(3));

        var onlinePeers = peers.Count(p =>
            p.LastHandshake.HasValue &&
            p.LastHandshake.Value >= onlineThreshold);

        var totalRx = peers.Sum(p => p.BytesReceived);
        var totalTx = peers.Sum(p => p.BytesSent);

        return Result<OnlineStatsResponse>.Success(
            new OnlineStatsResponse(
                peers.Count,
                onlinePeers,
                totalRx,
                totalTx,
                FormatBytes(totalRx),
                FormatBytes(totalTx)));
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        if (bytes < 1024 * 1024 * 1024)
            return $"{bytes / (1024.0 * 1024):F1} MB";
        return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
    }
}
