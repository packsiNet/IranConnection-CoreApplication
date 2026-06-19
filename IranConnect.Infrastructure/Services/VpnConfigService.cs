using IranConnect.Application.Common.Interfaces;
using IranConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace IranConnect.Infrastructure.Services;

public class VpnConfigService : IVpnConfigService
{
    private readonly IApplicationDbContext _context;
    private readonly IWireGuardService _wireGuardService;
    private readonly IConfiguration _configuration;
    private static readonly SemaphoreSlim _ipLock = new(1, 1);

    public VpnConfigService(
        IApplicationDbContext context,
        IWireGuardService wireGuardService,
        IConfiguration configuration)
    {
        _context = context;
        _wireGuardService = wireGuardService;
        _configuration = configuration;
    }

    public async Task<WireGuardClientConfig> GetOrCreatePeerAsync(
        Guid userId)
    {
        // Return existing peer if exists
        var existing = await _context.WireGuardPeers
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (existing != null)
            return BuildConfig(existing);

        // Create new peer
        await _ipLock.WaitAsync();
        try
        {
            // Double-check after lock
            existing = await _context.WireGuardPeers
                .FirstOrDefaultAsync(p => p.UserId == userId);
            if (existing != null)
                return BuildConfig(existing);

            var assignedIp = await GetNextAvailableIpAsync();
            var (publicKey, privateKey) =
                await _wireGuardService.GenerateKeyPairAsync();

            await _wireGuardService.AddPeerAsync(publicKey, assignedIp);

            var peer = WireGuardPeer.Create(
                userId, publicKey, privateKey, assignedIp);

            _context.WireGuardPeers.Add(peer);
            await _context.SaveChangesAsync(default);

            return BuildConfig(peer);
        }
        finally
        {
            _ipLock.Release();
        }
    }

    private async Task<string> GetNextAvailableIpAsync()
    {
        var usedIps = await _context.WireGuardPeers
            .Select(p => p.AssignedIp)
            .ToListAsync();

        // Start from 10.0.0.2 (10.0.0.1 is server)
        for (int i = 2; i <= 254; i++)
        {
            var candidate = $"10.0.0.{i}/32";
            if (!usedIps.Contains(candidate))
                return candidate;
        }

        // If 10.0.0.x is full, move to 10.0.1.x
        for (int i = 1; i <= 254; i++)
        {
            var candidate = $"10.0.1.{i}/32";
            if (!usedIps.Contains(candidate))
                return candidate;
        }

        throw new InvalidOperationException(
            "No available IP addresses");
    }

    private WireGuardClientConfig BuildConfig(WireGuardPeer peer)
    {
        return new WireGuardClientConfig(
            peer.PrivateKey,
            peer.AssignedIp,
            _configuration["WireGuard:ServerPublicKey"]!,
            _configuration["WireGuard:ServerEndpoint"]!,
            _configuration["WireGuard:Dns"] ?? "1.1.1.1");
    }
}
