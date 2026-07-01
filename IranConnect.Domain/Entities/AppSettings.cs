namespace IranConnect.Domain.Entities;

public class AppSettings
{
    public static readonly Guid SingletonId = new("00000000-0000-0000-0000-000000000010");

    public Guid Id { get; private set; } = SingletonId;
    public bool AdsEnabled { get; private set; } = true;

    // App version and the version marker for the bundled Iranian-apps catalog.
    // Clients compare these against their local values to decide on updates.
    public string Version { get; private set; } = "1.0.0";
    public string IranianAppsUpdateVersion { get; private set; } = "1.0.0";

    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    private AppSettings() { }

    public static AppSettings CreateDefault() => new();

    public void SetAdsEnabled(bool enabled)
    {
        AdsEnabled = enabled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetVersions(string version, string iranianAppsUpdateVersion)
    {
        Version = version.Trim();
        IranianAppsUpdateVersion = iranianAppsUpdateVersion.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
