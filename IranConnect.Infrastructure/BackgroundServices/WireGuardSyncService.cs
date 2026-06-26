using IranConnect.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IranConnect.Infrastructure.BackgroundServices;

// Runs once at app startup. Reads active peers from the DB and re-applies them
// onto the live wg0 interface. Without this, peers vanish from the interface
// after any service or wg-quick restart (they are managed dynamically, not
// stored statically in wg0.conf), breaking client handshakes until each peer
// is touched again.
public class WireGuardSyncService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WireGuardSyncService> _logger;

    public WireGuardSyncService(
        IServiceScopeFactory scopeFactory,
        ILogger<WireGuardSyncService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider
                .GetRequiredService<IApplicationDbContext>();
            var wireGuardService = scope.ServiceProvider
                .GetRequiredService<IWireGuardService>();

            var peers = await context.WireGuardPeers
                .Where(p => p.IsActive)
                .Select(p => new WireGuardPeerConfig(
                    p.PublicKey, p.AssignedIp))
                .ToListAsync(cancellationToken);

            if (peers.Count == 0)
            {
                _logger.LogInformation(
                    "WireGuard startup sync: no active peers in DB");
                return;
            }

            await wireGuardService.SyncPeersAsync(peers);
        }
        catch (Exception ex)
        {
            // Warn, do not crash app startup if wg is unavailable.
            _logger.LogWarning(ex,
                "WireGuard startup peer sync failed; " +
                "live interface may be out of sync with DB");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}
