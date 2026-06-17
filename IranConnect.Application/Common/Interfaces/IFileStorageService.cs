namespace IranConnect.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveAsync(byte[] fileBytes, string fileName, CancellationToken cancellationToken);
    Task DeleteAsync(string storedFileName, CancellationToken cancellationToken);
    string GetFullPath(string storedFileName);
}
