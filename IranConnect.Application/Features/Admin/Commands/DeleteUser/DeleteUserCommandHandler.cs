using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Admin.Commands.DeleteUser;

public class DeleteUserCommandHandler
    : IRequestHandler<DeleteUserCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IWireGuardService _wireGuardService;
    private readonly IFileStorageService _fileStorage;

    public DeleteUserCommandHandler(
        IApplicationDbContext context,
        IWireGuardService wireGuardService,
        IFileStorageService fileStorage)
    {
        _context = context;
        _wireGuardService = wireGuardService;
        _fileStorage = fileStorage;
    }

    public async Task<Result<string>> Handle(
        DeleteUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(
                u => u.Id == request.TargetUserId,
                cancellationToken);

        if (user is null)
            return Result<string>.Failure("User not found", 404);

        var peer = await _context.WireGuardPeers
            .FirstOrDefaultAsync(
                p => p.UserId == request.TargetUserId,
                cancellationToken);

        if (peer != null)
        {
            try
            {
                await _wireGuardService.RemovePeerAsync(peer.PublicKey);
            }
            catch (Exception)
            {
                // peer may already be removed from WireGuard
            }
            _context.WireGuardPeers.Remove(peer);
        }

        var receipts = await _context.PaymentReceipts
            .Where(r => r.UserId == request.TargetUserId)
            .ToListAsync(cancellationToken);

        foreach (var receipt in receipts)
        {
            try
            {
                await _fileStorage.DeleteAsync(
                    receipt.StoredFileName, cancellationToken);
            }
            catch { /* file may not exist */ }
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success("User deleted successfully");
    }
}
