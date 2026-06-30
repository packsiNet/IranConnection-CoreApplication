namespace IranConnect.Domain.Entities;

public class AppSettings
{
    public static readonly Guid SingletonId = new("00000000-0000-0000-0000-000000000010");

    public Guid Id { get; private set; } = SingletonId;
    public bool AdsEnabled { get; private set; } = true;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    private AppSettings() { }

    public static AppSettings CreateDefault() => new();

    public void SetAdsEnabled(bool enabled)
    {
        AdsEnabled = enabled;
        UpdatedAt = DateTime.UtcNow;
    }
}
