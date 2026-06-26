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

        // Parse configured subnet (e.g. "10.0.0.0/24") to derive base octets.
        // Server always occupies .1; clients start at .2.
        var subnet = _configuration["WireGuard:Subnet"] ?? "10.0.0.0/24";
        var baseOctets = subnet.Split('/')[0].Split('.');
        var o1 = baseOctets[0];
        var o2 = baseOctets[1];
        var o3 = baseOctets[2];

        // /24 → 253 clients (.2 … .254). Extend subnet to /23 in config for more.
        for (int i = 2; i <= 254; i++)
        {
            var candidate = $"{o1}.{o2}.{o3}.{i}/32";
            if (!usedIps.Contains(candidate))
                return candidate;
        }

        throw new InvalidOperationException(
            $"No available IP addresses in subnet {subnet}. " +
            "Extend WireGuard:Subnet to a larger range (e.g. 10.0.0.0/16).");
    }

    private WireGuardClientConfig BuildConfig(WireGuardPeer peer)
    {
        return new WireGuardClientConfig(
            peer.PrivateKey,
            peer.AssignedIp,
            _configuration["WireGuard:ServerPublicKey"]!,
            _configuration["WireGuard:ServerEndpoint"]!,
            _configuration["WireGuard:Dns"] ?? "1.1.1.1",
            // Full tunnel — per-app split tunneling enforced by Android VpnService
            _configuration["WireGuard:ClientAllowedIPs"] ?? "0.0.0.0/0, ::/0",
            BuildObfuscation());
    }

    // AmneziaWG params served to every client. Defaults match the values the
    // server setup script writes; production overrides come from appsettings.
    private AmneziaObfuscation BuildObfuscation()
    {
        return new AmneziaObfuscation(
            ReadInt("Jc", 8),
            ReadInt("Jmin", 24),
            ReadInt("Jmax", 80),
            ReadInt("S1", 24),
            ReadInt("S2", 48),
            ReadLong("H1", 1148364632),
            ReadLong("H2", 776863562),
            ReadLong("H3", 1818526299),
            ReadLong("H4", 1971479911));
    }

    private int ReadInt(string key, int fallback)
        => int.TryParse(_configuration[$"WireGuard:Obfuscation:{key}"],
            out var v) ? v : fallback;

    private long ReadLong(string key, long fallback)
        => long.TryParse(_configuration[$"WireGuard:Obfuscation:{key}"],
            out var v) ? v : fallback;
}
