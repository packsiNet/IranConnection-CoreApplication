using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using IranConnect.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IranConnect.Application.Features.Admin.Commands.FactoryReset;

public class FactoryResetCommandHandler
    : IRequestHandler<FactoryResetCommand, Result<FactoryResetResult>>
{
    // Salted PBKDF2 hash (SHA256, 100k iters) of the factory-reset password.
    // The plaintext is never stored; verified with IPasswordHasher.Verify.
    private const string ExpectedPasswordHash =
        "ibaOVg4O9qCNITYXI88bdA==.bklhtAUh18ipEx1ldCGSQJsraS9PVI4CmvCZ6kW6yJQ=";

    private readonly IApplicationDbContext _context;
    private readonly IWireGuardService _wireGuardService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<FactoryResetCommandHandler> _logger;

    public FactoryResetCommandHandler(
        IApplicationDbContext context,
        IWireGuardService wireGuardService,
        IPasswordHasher passwordHasher,
        ILogger<FactoryResetCommandHandler> logger)
    {
        _context = context;
        _wireGuardService = wireGuardService;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<Result<FactoryResetResult>> Handle(
        FactoryResetCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Password) ||
            !_passwordHasher.Verify(request.Password, ExpectedPasswordHash))
        {
            _logger.LogWarning("Factory reset attempted with invalid password");
            return Result<FactoryResetResult>.Failure("رمز نامعتبر است", 401);
        }

        _logger.LogWarning("FACTORY RESET starting — wiping all user data");

        // 1) Tear down live WireGuard peers before dropping their DB rows,
        //    otherwise the interface keeps orphaned peers until next restart.
        var peerKeys = await _context.WireGuardPeers
            .Select(p => p.PublicKey)
            .ToListAsync(cancellationToken);

        var removedFromInterface = 0;
        foreach (var publicKey in peerKeys)
        {
            try
            {
                await _wireGuardService.RemovePeerAsync(publicKey);
                removedFromInterface++;
            }
            catch (Exception ex)
            {
                // Keep going — DB row is deleted below regardless.
                _logger.LogWarning(ex,
                    "Failed to remove peer {PublicKey} from wg interface", publicKey);
            }
        }

        // 2) Wipe tables. EF orders the deletes to respect FK constraints on
        //    SaveChanges. Admin accounts are preserved so the panel stays usable.
        _context.RefreshTokens.RemoveRange(
            await _context.RefreshTokens.ToListAsync(cancellationToken));
        _context.PaymentReceipts.RemoveRange(
            await _context.PaymentReceipts.ToListAsync(cancellationToken));
        _context.Payments.RemoveRange(
            await _context.Payments.ToListAsync(cancellationToken));
        _context.Reviews.RemoveRange(
            await _context.Reviews.ToListAsync(cancellationToken));
        _context.StatEvents.RemoveRange(
            await _context.StatEvents.ToListAsync(cancellationToken));

        var peers = await _context.WireGuardPeers.ToListAsync(cancellationToken);
        _context.WireGuardPeers.RemoveRange(peers);

        _context.Subscriptions.RemoveRange(
            await _context.Subscriptions.ToListAsync(cancellationToken));

        var nonAdminUsers = await _context.Users
            .Where(u => !u.IsAdmin)
            .ToListAsync(cancellationToken);
        _context.Users.RemoveRange(nonAdminUsers);

        // 3) Reset app settings to defaults (e.g. ads back on).
        _context.AppSettings.RemoveRange(
            await _context.AppSettings.ToListAsync(cancellationToken));
        _context.AppSettings.Add(AppSettings.CreateDefault());

        await _context.SaveChangesAsync(cancellationToken);

        var peersRemoved = peers.Count;
        var usersDeleted = nonAdminUsers.Count;

        _logger.LogWarning(
            "FACTORY RESET done — {Users} users, {Peers} peers deleted",
            usersDeleted, peersRemoved);

        return Result<FactoryResetResult>.Success(
            new FactoryResetResult(usersDeleted, peersRemoved, removedFromInterface));
    }
}
