using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Admin.Queries.GetPeerStats;

public class GetPeerStatsQueryHandler
    : IRequestHandler<GetPeerStatsQuery, Result<List<PeerStatsResponse>>>
{
    private readonly IApplicationDbContext _context;

    public GetPeerStatsQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<List<PeerStatsResponse>>> Handle(
        GetPeerStatsQuery request,
        CancellationToken cancellationToken)
    {
        var peers = await _context.WireGuardPeers
            .Include(p => p.User)
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.LastSeenAt)
            .ToListAsync(cancellationToken);

        var result = peers.Select(p => new PeerStatsResponse(
            p.UserId.ToString(),
            p.User.Email,
            p.AssignedIp,
            p.PublicKey[..8] + "...",
            p.IsOnline,
            p.BytesReceived,
            p.BytesSent,
            FormatBytes(p.BytesReceived),
            FormatBytes(p.BytesSent),
            p.LastHandshake,
            p.LastSeenAt)).ToList();

        return Result<List<PeerStatsResponse>>.Success(result);
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
