using IranConnect.Domain.Common;

namespace IranConnect.Domain.Entities;

public class Review : BaseEntity
{
    public string FullName { get; private set; } = default!;
    public int Rating { get; private set; }
    public string? Comment { get; private set; }
    public string? IpAddress { get; private set; }
    public string? CountryCode { get; private set; }
    public bool IsApproved { get; private set; }

    private Review() { }

    public static Review Create(
        string fullName,
        int rating,
        string? comment,
        string? ipAddress,
        string? countryCode)
    {
        return new Review
        {
            FullName = fullName.Trim(),
            Rating = rating,
            Comment = comment?.Trim(),
            IpAddress = ipAddress,
            CountryCode = countryCode,
            IsApproved = false
        };
    }

    public void Approve() => IsApproved = true;
    public void Reject() => IsApproved = false;
}
