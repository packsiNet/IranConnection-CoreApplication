using IranConnect.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IranConnect.Infrastructure.BackgroundServices;

public class WireGuardStatsService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WireGuardStatsService> _logger;

    public WireGuardStatsService(
        IServiceScopeFactory scopeFactory,
        ILogger<WireGuardStatsService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await UpdateStatsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update WireGuard stats");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task UpdateStatsAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider
            .GetRequiredService<IApplicationDbContext>();
        var wireGuardService = scope.ServiceProvider
            .GetRequiredService<IWireGuardService>();

        var stats = await wireGuardService.GetAllPeerStatsAsync();
        var peers = await context.WireGuardPeers.ToListAsync();

        foreach (var stat in stats)
        {
            var peer = peers.FirstOrDefault(
                p => p.PublicKey == stat.PublicKey);
            if (peer is null) continue;

            peer.UpdateStats(
                stat.BytesReceived,
                stat.BytesSent,
                stat.LastHandshake);
        }

        await context.SaveChangesAsync(default);

        await CheckBandwidthLimitsAsync(context, wireGuardService);
    }

    private async Task CheckBandwidthLimitsAsync(
        IApplicationDbContext context,
        IWireGuardService wireGuardService)
    {
        var exceededPeers = await context.WireGuardPeers
            .Where(p =>
                p.IsActive &&
                p.BandwidthLimitBytes.HasValue &&
                (p.BytesReceived + p.BytesSent) >= p.BandwidthLimitBytes.Value)
            .ToListAsync();

        foreach (var peer in exceededPeers)
        {
            try
            {
                await wireGuardService.RemovePeerAsync(peer.PublicKey);
                peer.Deactivate();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to remove exceeded peer {PeerId}", peer.Id);
            }
        }

        if (exceededPeers.Any())
            await context.SaveChangesAsync(default);
    }
}
