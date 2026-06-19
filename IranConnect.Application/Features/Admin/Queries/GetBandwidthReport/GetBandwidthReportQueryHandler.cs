using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using IranConnect.Application.Features.Admin.Queries.GetUsers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Admin.Queries.GetBandwidthReport;

public class GetBandwidthReportQueryHandler
    : IRequestHandler<GetBandwidthReportQuery, Result<PagedResult<BandwidthReportItem>>>
{
    private readonly IApplicationDbContext _context;

    public GetBandwidthReportQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<PagedResult<BandwidthReportItem>>> Handle(
        GetBandwidthReportQuery request,
        CancellationToken cancellationToken)
    {
        var peers = await _context.WireGuardPeers
            .Include(p => p.User)
            .Where(p => p.IsActive)
            .ToListAsync(cancellationToken);

        var totalCount = peers.Count;

        IEnumerable<BandwidthReportItem> items = peers.Select(p => new BandwidthReportItem(
            p.UserId.ToString(),
            p.User.Email,
            p.AssignedIp,
            p.IsOnline,
            p.BytesReceived,
            p.BytesSent,
            p.TotalBytesUsed,
            FormatBytes(p.BytesReceived),
            FormatBytes(p.BytesSent),
            FormatBytes(p.TotalBytesUsed),
            p.BandwidthLimitBytes,
            p.BandwidthLimitBytes.HasValue
                ? FormatBytes(p.BandwidthLimitBytes.Value)
                : null,
            p.HasExceededBandwidth,
            p.LastSeenAt));

        items = request.SortBy?.ToLower() switch
        {
            "received" => items.OrderByDescending(i => i.BytesReceived),
            "sent" => items.OrderByDescending(i => i.BytesSent),
            _ => items.OrderByDescending(i => i.TotalBytes)
        };

        var paged = items
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var totalPages = (int)Math.Ceiling(
            totalCount / (double)request.PageSize);

        return Result<PagedResult<BandwidthReportItem>>.Success(
            new PagedResult<BandwidthReportItem>(
                paged, totalCount,
                request.Page, request.PageSize, totalPages));
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
