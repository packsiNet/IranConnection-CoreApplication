using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Admin.Queries.GetPeerDetail;

public class GetPeerDetailQueryHandler
    : IRequestHandler<GetPeerDetailQuery, Result<PeerDetailResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetPeerDetailQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<PeerDetailResponse>> Handle(
        GetPeerDetailQuery request,
        CancellationToken cancellationToken)
    {
        var peer = await _context.WireGuardPeers
            .Include(p => p.User)
            .FirstOrDefaultAsync(
                p => p.UserId == request.TargetUserId,
                cancellationToken);

        if (peer is null)
            return Result<PeerDetailResponse>.Failure("Peer not found", 404);

        return Result<PeerDetailResponse>.Success(
            new PeerDetailResponse(
                peer.UserId.ToString(),
                peer.User.Email,
                peer.AssignedIp,
                peer.PublicKey,
                peer.IsOnline,
                peer.IsActive,
                peer.BytesReceived,
                peer.BytesSent,
                FormatBytes(peer.BytesReceived),
                FormatBytes(peer.BytesSent),
                peer.LastHandshake,
                peer.LastSeenAt,
                peer.CreatedAt,
                peer.BandwidthLimitBytes,
                peer.BandwidthLimitBytes.HasValue
                    ? FormatBytes(peer.BandwidthLimitBytes.Value)
                    : null));
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
