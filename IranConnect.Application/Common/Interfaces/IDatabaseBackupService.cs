namespace IranConnect.Application.Common.Interfaces;

/// <summary>Metadata for a single database backup file.</summary>
public record BackupFileInfo(
    string FileName,
    long SizeBytes,
    DateTime CreatedAtUtc);

/// <summary>
/// Creates, lists, restores and deletes PostgreSQL database backups.
/// Admin-only. Restore is destructive and overwrites current data.
/// </summary>
public interface IDatabaseBackupService
{
    /// <summary>Runs pg_dump and stores a new backup file. Returns its metadata.</summary>
    Task<BackupFileInfo> CreateBackupAsync(CancellationToken cancellationToken);

    /// <summary>Lists existing backup files, newest first.</summary>
    IReadOnlyList<BackupFileInfo> ListBackups();

    /// <summary>Restores the database from the given backup file (destructive).</summary>
    Task RestoreBackupAsync(string fileName, CancellationToken cancellationToken);

    /// <summary>Deletes a backup file. Returns false if it does not exist.</summary>
    bool DeleteBackup(string fileName);

    /// <summary>True if the named backup file exists in the backup directory.</summary>
    bool Exists(string fileName);

    /// <summary>Absolute path of a backup file (for download).</summary>
    string GetFullPath(string fileName);
}
