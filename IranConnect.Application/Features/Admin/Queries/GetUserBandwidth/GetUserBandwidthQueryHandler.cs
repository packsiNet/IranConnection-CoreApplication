using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Admin.Queries.GetUserBandwidth;

public class GetUserBandwidthQueryHandler
    : IRequestHandler<GetUserBandwidthQuery, Result<UserBandwidthResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetUserBandwidthQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<UserBandwidthResponse>> Handle(
        GetUserBandwidthQuery request,
        CancellationToken cancellationToken)
    {
        var peer = await _context.WireGuardPeers
            .Include(p => p.User)
            .FirstOrDefaultAsync(
                p => p.UserId == request.TargetUserId,
                cancellationToken);

        if (peer is null)
            return Result<UserBandwidthResponse>.Failure("Peer not found", 404);

        double? usagePercent = peer.BandwidthLimitBytes.HasValue
            ? Math.Round(
                peer.TotalBytesUsed * 100.0 /
                peer.BandwidthLimitBytes.Value, 1)
            : null;

        return Result<UserBandwidthResponse>.Success(
            new UserBandwidthResponse(
                peer.UserId.ToString(),
                peer.User.Email,
                peer.BytesReceived,
                peer.BytesSent,
                peer.TotalBytesUsed,
                FormatBytes(peer.BytesReceived),
                FormatBytes(peer.BytesSent),
                FormatBytes(peer.TotalBytesUsed),
                peer.BandwidthLimitBytes,
                peer.BandwidthLimitBytes.HasValue
                    ? FormatBytes(peer.BandwidthLimitBytes.Value)
                    : null,
                usagePercent,
                peer.HasExceededBandwidth));
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
