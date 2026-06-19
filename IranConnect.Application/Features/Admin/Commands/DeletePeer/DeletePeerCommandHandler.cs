using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Admin.Commands.DeletePeer;

public class DeletePeerCommandHandler
    : IRequestHandler<DeletePeerCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IWireGuardService _wireGuardService;

    public DeletePeerCommandHandler(
        IApplicationDbContext context,
        IWireGuardService wireGuardService)
    {
        _context = context;
        _wireGuardService = wireGuardService;
    }

    public async Task<Result<string>> Handle(
        DeletePeerCommand request,
        CancellationToken cancellationToken)
    {
        var peer = await _context.WireGuardPeers
            .FirstOrDefaultAsync(
                p => p.UserId == request.TargetUserId,
                cancellationToken);

        if (peer is null)
            return Result<string>.Failure("Peer not found", 404);

        await _wireGuardService.RemovePeerAsync(peer.PublicKey);
        _context.WireGuardPeers.Remove(peer);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success("Peer deleted successfully");
    }
}
