using System.Diagnostics;
using IranConnect.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IranConnect.Infrastructure.Services;

public class WireGuardService : IWireGuardService
{
    private readonly ILogger<WireGuardService> _logger;
    private readonly IConfiguration _configuration;

    public WireGuardService(
        ILogger<WireGuardService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    // AmneziaWG ships `awg` / `awg-quick` (drop-in for wg / wg-quick) with DPI
    // evasion. Configurable so plain WireGuard ("wg") can still be used in dev.
    private string Wg => _configuration["WireGuard:Binary"] ?? "awg";
    private string WgQuick =>
        _configuration["WireGuard:QuickBinary"] ?? "awg-quick";

    public async Task<(string publicKey, string privateKey)>
        GenerateKeyPairAsync()
    {
        var privateKey = (await RunCommandAsync($"sudo {Wg} genkey")).Trim();
        // WireGuard keys are base64 — safe inside single-quotes
        var publicKey = (await RunCommandAsync(
            $"echo '{privateKey}' | sudo {Wg} pubkey")).Trim();
        return (publicKey, privateKey);
    }

    public async Task AddPeerAsync(string publicKey, string assignedIp)
    {
        var ip = assignedIp.Split('/')[0];
        var iface = _configuration["WireGuard:Interface"] ?? "wg0";

        await RunCommandAsync(
            $"sudo {Wg} set {iface} peer {publicKey} allowed-ips {ip}/32");
        await RunCommandAsync($"sudo {WgQuick} save {iface}");

        _logger.LogInformation(
            "WireGuard peer added: {PublicKey} -> {Ip}",
            publicKey[..8] + "...", assignedIp);
    }

    public async Task SyncPeersAsync(
        IReadOnlyCollection<WireGuardPeerConfig> peers)
    {
        var iface = _configuration["WireGuard:Interface"] ?? "wg0";
        var applied = 0;

        foreach (var peer in peers)
        {
            try
            {
                var ip = peer.AssignedIp.Split('/')[0];
                // Idempotent: overwrites existing peer's allowed-ips if present.
                await RunCommandAsync(
                    $"sudo {Wg} set {iface} peer {peer.PublicKey} " +
                    $"allowed-ips {ip}/32");
                applied++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to sync WireGuard peer {PublicKey} -> {Ip}",
                    peer.PublicKey.Length >= 8
                        ? peer.PublicKey[..8] + "..." : peer.PublicKey,
                    peer.AssignedIp);
            }
        }

        // Persist runtime peers to wg0.conf once after the batch.
        if (applied > 0)
        {
            try
            {
                await RunCommandAsync($"sudo {WgQuick} save {iface}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "wg-quick save failed after peer sync on {Iface}", iface);
            }
        }

        _logger.LogInformation(
            "WireGuard peer sync complete: {Applied}/{Total} peers applied " +
            "to {Iface}", applied, peers.Count, iface);
    }

    public async Task RemovePeerAsync(string publicKey)
    {
        var iface = _configuration["WireGuard:Interface"] ?? "wg0";

        await RunCommandAsync($"sudo {Wg} set {iface} peer {publicKey} remove");
        await RunCommandAsync($"sudo {WgQuick} save {iface}");

        _logger.LogInformation(
            "WireGuard peer removed: {PublicKey}",
            publicKey[..8] + "...");
    }

    public async Task<List<WireGuardPeerStats>> GetAllPeerStatsAsync()
    {
        var iface = _configuration["WireGuard:Interface"] ?? "wg0";
        var output = await RunCommandAsync($"sudo {Wg} show {iface} dump");
        var stats = new List<WireGuardPeerStats>();

        foreach (var line in output.Split('\n').Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var parts = line.Split('\t');
            if (parts.Length < 7) continue;

            // Format: publicKey, presharedKey, endpoint,
            //         allowedIps, latestHandshake,
            //         rxBytes, txBytes, persistentKeepalive
            var publicKey = parts[0];
            var endpoint = parts[2];
            var lastHandshakeUnix = long.TryParse(parts[4], out var ts)
                ? ts : 0;
            var rxBytes = long.TryParse(parts[5], out var rx) ? rx : 0;
            var txBytes = long.TryParse(parts[6], out var tx) ? tx : 0;

            DateTime? lastHandshake = lastHandshakeUnix > 0
                ? DateTimeOffset.FromUnixTimeSeconds(lastHandshakeUnix)
                    .UtcDateTime
                : null;

            stats.Add(new WireGuardPeerStats(
                publicKey, rxBytes, txBytes, lastHandshake, endpoint));
        }

        return stats;
    }

    private async Task<string> RunCommandAsync(string command)
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = $"-c \"{command}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(processInfo)!;
        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0 && !string.IsNullOrEmpty(error))
        {
            _logger.LogError(
                "WireGuard command failed (exit {ExitCode}): {Command} | {Error}",
                process.ExitCode, command, error.Trim());
            throw new InvalidOperationException(
                $"WireGuard command failed: {error}");
        }

        return output;
    }
}
