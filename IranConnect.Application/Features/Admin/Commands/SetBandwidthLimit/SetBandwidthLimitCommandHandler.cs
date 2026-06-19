using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Admin.Commands.SetBandwidthLimit;

public class SetBandwidthLimitCommandHandler
    : IRequestHandler<SetBandwidthLimitCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public SetBandwidthLimitCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<string>> Handle(
        SetBandwidthLimitCommand request,
        CancellationToken cancellationToken)
    {
        var peer = await _context.WireGuardPeers
            .FirstOrDefaultAsync(
                p => p.UserId == request.TargetUserId,
                cancellationToken);

        if (peer is null)
            return Result<string>.Failure("Peer not found", 404);

        peer.SetBandwidthLimit(request.LimitBytes);
        await _context.SaveChangesAsync(cancellationToken);

        var message = request.LimitBytes.HasValue
            ? $"Bandwidth limit set to {FormatBytes(request.LimitBytes.Value)}"
            : "Bandwidth limit removed";

        return Result<string>.Success(message);
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        if (bytes < 1024 * 1024 * 1024)
            return $"{bytes / (1024.0 * 1024):F1} MB";
        return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
    }
}
