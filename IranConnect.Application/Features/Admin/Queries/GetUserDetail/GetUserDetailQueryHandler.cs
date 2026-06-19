using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Admin.Queries.GetUserDetail;

public class GetUserDetailQueryHandler
    : IRequestHandler<GetUserDetailQuery, Result<UserDetailResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetUserDetailQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<UserDetailResponse>> Handle(
        GetUserDetailQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Subscription)
            .FirstOrDefaultAsync(
                u => u.Id == request.TargetUserId,
                cancellationToken);

        if (user is null)
            return Result<UserDetailResponse>.Failure("User not found", 404);

        var peer = await _context.WireGuardPeers
            .FirstOrDefaultAsync(
                p => p.UserId == request.TargetUserId,
                cancellationToken);

        var receipts = await _context.PaymentReceipts
            .Where(r => r.UserId == request.TargetUserId)
            .OrderByDescending(r => r.CreatedAt)
            .Take(10)
            .Select(r => new ReceiptSummary(
                r.Id.ToString(),
                r.Status.ToString(),
                r.RequestedDurationDays,
                r.CreatedAt,
                r.ReviewedAt))
            .ToListAsync(cancellationToken);

        return Result<UserDetailResponse>.Success(new UserDetailResponse(
            user.Id.ToString(),
            user.Email,
            user.FullName,
            user.IsEmailVerified,
            user.IsActive,
            user.IsDeviceUser,
            user.CreatedAt,
            user.LastLoginAt,
            new SubscriptionDetail(
                user.Subscription?.Plan.ToString() ?? "Free",
                user.Subscription?.Status.ToString() ?? "Active",
                user.Subscription?.StartDate ?? user.CreatedAt,
                user.Subscription?.ExpireDate ?? DateTime.UtcNow.AddYears(100),
                user.Subscription?.DaysRemaining ?? 0,
                user.Subscription?.IsActive ?? false),
            peer is null ? null : new WireGuardPeerDetail(
                peer.AssignedIp,
                peer.PublicKey[..8] + "...",
                peer.IsOnline,
                FormatBytes(peer.BytesReceived),
                FormatBytes(peer.BytesSent),
                peer.LastHandshake,
                peer.LastSeenAt),
            receipts));
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
