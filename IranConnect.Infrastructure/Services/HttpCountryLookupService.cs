using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using IranConnect.Application.Common.Interfaces;

namespace IranConnect.Infrastructure.Services;

public class HttpCountryLookupService : ICountryLookupService
{
    private readonly HttpClient _httpClient;

    private static readonly HashSet<string> LocalAddresses =
    [
        "127.0.0.1", "::1", "localhost", "0.0.0.1"
    ];

    public HttpCountryLookupService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(3);
    }

    public async Task<string?> GetCountryCodeAsync(
        string ipAddress,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(ipAddress) || LocalAddresses.Contains(ipAddress))
            return null;

        if (IsPrivateRange(ipAddress))
            return null;

        try
        {
            var response = await _httpClient.GetFromJsonAsync<IpApiResponse>(
                $"http://ip-api.com/json/{ipAddress}?fields=status,countryCode",
                cancellationToken);

            return response?.Status == "success" ? response.CountryCode : null;
        }
        catch
        {
            return null;
        }
    }

    private static bool IsPrivateRange(string ipAddress)
    {
        if (!IPAddress.TryParse(ipAddress, out var ip)) return false;
        var bytes = ip.GetAddressBytes();
        if (bytes.Length != 4) return false;

        return bytes[0] == 10
            || (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
            || (bytes[0] == 192 && bytes[1] == 168);
    }

    private record IpApiResponse(
        [property: JsonPropertyName("status")] string Status,
        [property: JsonPropertyName("countryCode")] string? CountryCode);
}
