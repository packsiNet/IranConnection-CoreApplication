using IranConnect.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace IranConnect.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public LocalFileStorageService(IConfiguration configuration)
    {
        _basePath = configuration["FileStorage:ReceiptsPath"] ?? "uploads/receipts";
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveAsync(
        byte[] fileBytes,
        string fileName,
        CancellationToken cancellationToken)
    {
        var ext = Path.GetExtension(fileName);
        var storedFileName = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(_basePath, storedFileName);
        await File.WriteAllBytesAsync(fullPath, fileBytes, cancellationToken);
        return storedFileName;
    }

    public Task DeleteAsync(string storedFileName, CancellationToken cancellationToken)
    {
        var fullPath = Path.Combine(_basePath, storedFileName);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        return Task.CompletedTask;
    }

    public string GetFullPath(string storedFileName)
        => Path.Combine(_basePath, storedFileName);
}
