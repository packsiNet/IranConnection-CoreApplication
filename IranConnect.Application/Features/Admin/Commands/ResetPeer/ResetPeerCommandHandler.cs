using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using IranConnect.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Admin.Commands.ResetPeer;

public class ResetPeerCommandHandler
    : IRequestHandler<ResetPeerCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IWireGuardService _wireGuardService;

    public ResetPeerCommandHandler(
        IApplicationDbContext context,
        IWireGuardService wireGuardService)
    {
        _context = context;
        _wireGuardService = wireGuardService;
    }

    public async Task<Result<string>> Handle(
        ResetPeerCommand request,
        CancellationToken cancellationToken)
    {
        var peer = await _context.WireGuardPeers
            .FirstOrDefaultAsync(
                p => p.UserId == request.TargetUserId,
                cancellationToken);

        if (peer is null)
            return Result<string>.Failure("Peer not found", 404);

        var oldIp = peer.AssignedIp;

        await _wireGuardService.RemovePeerAsync(peer.PublicKey);
        _context.WireGuardPeers.Remove(peer);
        await _context.SaveChangesAsync(cancellationToken);

        var (publicKey, privateKey) =
            await _wireGuardService.GenerateKeyPairAsync();

        await _wireGuardService.AddPeerAsync(publicKey, oldIp);

        var newPeer = WireGuardPeer.Create(
            request.TargetUserId,
            publicKey,
            privateKey,
            oldIp);

        _context.WireGuardPeers.Add(newPeer);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(
            "Peer reset successfully. User must re-fetch config");
    }
}
