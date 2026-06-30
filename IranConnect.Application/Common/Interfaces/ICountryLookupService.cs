namespace IranConnect.Application.Common.Interfaces;

public interface ICountryLookupService
{
    Task<string?> GetCountryCodeAsync(string ipAddress, CancellationToken cancellationToken);
}
