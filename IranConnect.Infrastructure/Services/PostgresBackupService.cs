using System.Diagnostics;
using IranConnect.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace IranConnect.Infrastructure.Services;

/// <summary>
/// PostgreSQL backup service built on the pg_dump / pg_restore CLI tools.
/// Backups use the custom archive format (-Fc) so pg_restore can rebuild
/// the schema and data with --clean.
/// </summary>
public class PostgresBackupService : IDatabaseBackupService
{
    private const string BackupExtension = ".dump";

    private readonly string _basePath;
    private readonly string _pgDumpPath;
    private readonly string _pgRestorePath;
    private readonly string _host;
    private readonly int _port;
    private readonly string _database;
    private readonly string _username;
    private readonly string _password;

    public PostgresBackupService(IConfiguration configuration)
    {
        _basePath = configuration["Backup:Path"] ?? "backups";
        Directory.CreateDirectory(_basePath);

        // Allow overriding binary locations (e.g. full path on the server).
        _pgDumpPath = configuration["Backup:PgDumpPath"] ?? "pg_dump";
        _pgRestorePath = configuration["Backup:PgRestorePath"] ?? "pg_restore";

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection not configured");
        var csb = new NpgsqlConnectionStringBuilder(connectionString);

        _host = csb.Host ?? "localhost";
        _port = csb.Port;
        _database = csb.Database
            ?? throw new InvalidOperationException("Database name missing from connection string");
        _username = csb.Username ?? "postgres";
        _password = csb.Password ?? string.Empty;
    }

    public async Task<BackupFileInfo> CreateBackupAsync(CancellationToken cancellationToken)
    {
        var fileName = $"backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}{BackupExtension}";
        var fullPath = Path.Combine(_basePath, fileName);

        // -Fc = custom archive format, -Z 9 = max compression, -f = output file.
        var args = new[]
        {
            "-Fc", "-Z", "9",
            "-h", _host,
            "-p", _port.ToString(),
            "-U", _username,
            "-d", _database,
            "-f", fullPath
        };

        try
        {
            await RunProcessAsync(_pgDumpPath, args, cancellationToken);
        }
        catch
        {
            // Do not leave a half-written file behind on failure.
            if (File.Exists(fullPath)) File.Delete(fullPath);
            throw;
        }

        var info = new FileInfo(fullPath);
        return new BackupFileInfo(fileName, info.Length, info.CreationTimeUtc);
    }

    public IReadOnlyList<BackupFileInfo> ListBackups()
    {
        var dir = new DirectoryInfo(_basePath);
        if (!dir.Exists) return Array.Empty<BackupFileInfo>();

        return dir.GetFiles($"*{BackupExtension}")
            .OrderByDescending(f => f.CreationTimeUtc)
            .Select(f => new BackupFileInfo(f.Name, f.Length, f.CreationTimeUtc))
            .ToList();
    }

    public async Task RestoreBackupAsync(string fileName, CancellationToken cancellationToken)
    {
        var fullPath = ResolveSafePath(fileName);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("Backup file not found", fileName);

        // --clean --if-exists drops existing objects before recreating them.
        // --no-owner avoids failures when the restore role differs from dump owner.
        var args = new[]
        {
            "--clean", "--if-exists", "--no-owner",
            "-h", _host,
            "-p", _port.ToString(),
            "-U", _username,
            "-d", _database,
            fullPath
        };

        await RunProcessAsync(_pgRestorePath, args, cancellationToken);
    }

    public bool DeleteBackup(string fileName)
    {
        var fullPath = ResolveSafePath(fileName);
        if (!File.Exists(fullPath)) return false;
        File.Delete(fullPath);
        return true;
    }

    public bool Exists(string fileName)
    {
        try
        {
            return File.Exists(ResolveSafePath(fileName));
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    public string GetFullPath(string fileName) => ResolveSafePath(fileName);

    /// <summary>
    /// Resolves a caller-supplied file name to a path inside the backup
    /// directory, rejecting path traversal (e.g. "../../etc/passwd").
    /// </summary>
    private string ResolveSafePath(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name is required", nameof(fileName));

        // Only accept a bare file name — no directory components.
        var safeName = Path.GetFileName(fileName);
        if (safeName != fileName || Path.GetExtension(safeName) != BackupExtension)
            throw new ArgumentException("Invalid backup file name", nameof(fileName));

        return Path.Combine(_basePath, safeName);
    }

    private async Task RunProcessAsync(
        string fileName,
        string[] arguments,
        CancellationToken cancellationToken)
    {
        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        foreach (var arg in arguments)
            psi.ArgumentList.Add(arg);

        // Supply the password out-of-band so it never appears on the command line.
        psi.Environment["PGPASSWORD"] = _password;

        using var process = new Process { StartInfo = psi };

        try
        {
            process.Start();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to start '{fileName}'. Ensure PostgreSQL client tools are installed.", ex);
        }

        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
        var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);
        var stderr = await stderrTask;
        await stdoutTask;

        if (process.ExitCode != 0)
            throw new InvalidOperationException(
                $"'{fileName}' exited with code {process.ExitCode}: {stderr}");
    }
}
