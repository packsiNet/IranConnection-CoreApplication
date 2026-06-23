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

    public async Task<(string publicKey, string privateKey)>
        GenerateKeyPairAsync()
    {
        var privateKey = (await RunCommandAsync("sudo wg genkey")).Trim();
        // WireGuard keys are base64 — safe inside single-quotes
        var publicKey = (await RunCommandAsync(
            $"echo '{privateKey}' | sudo wg pubkey")).Trim();
        return (publicKey, privateKey);
    }

    public async Task AddPeerAsync(string publicKey, string assignedIp)
    {
        var ip = assignedIp.Split('/')[0];
        var iface = _configuration["WireGuard:Interface"] ?? "wg0";

        await RunCommandAsync(
            $"sudo wg set {iface} peer {publicKey} allowed-ips {ip}/32");
        await RunCommandAsync($"sudo wg-quick save {iface}");

        _logger.LogInformation(
            "WireGuard peer added: {PublicKey} -> {Ip}",
            publicKey[..8] + "...", assignedIp);
    }

    public async Task RemovePeerAsync(string publicKey)
    {
        var iface = _configuration["WireGuard:Interface"] ?? "wg0";

        await RunCommandAsync($"sudo wg set {iface} peer {publicKey} remove");
        await RunCommandAsync($"sudo wg-quick save {iface}");

        _logger.LogInformation(
            "WireGuard peer removed: {PublicKey}",
            publicKey[..8] + "...");
    }

    public async Task<List<WireGuardPeerStats>> GetAllPeerStatsAsync()
    {
        var iface = _configuration["WireGuard:Interface"] ?? "wg0";
        var output = await RunCommandAsync($"sudo wg show {iface} dump");
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
            throw new InvalidOperationException(
                $"WireGuard command failed: {error}");

        return output;
    }
}
