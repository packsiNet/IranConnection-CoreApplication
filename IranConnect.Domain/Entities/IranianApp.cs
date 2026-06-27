using IranConnect.Domain.Common;

namespace IranConnect.Domain.Entities;

// Allowed Iranian app the client routes through the tunnel (per-app split
// tunneling). Previously a hardcoded in-memory list; now DB-backed so admins
// can add / edit / remove apps and toggle Free vs Premium tier at runtime.
public class IranianApp : BaseEntity
{
    public string PackageName { get; private set; } = default!; // applicationId
    public string NameEn { get; private set; } = default!;
    public string NameFa { get; private set; } = default!;      // Title (فارسی)
    public bool IsFree { get; private set; }                    // tier: Free/Premium
    public bool IsActive { get; private set; } = true;

    private IranianApp() { }

    public static IranianApp Create(
        string packageName, string nameEn, string nameFa, bool isFree)
        => new()
        {
            PackageName = packageName.Trim(),
            NameEn = nameEn.Trim(),
            NameFa = nameFa.Trim(),
            IsFree = isFree,
            IsActive = true
        };

    public void Update(string packageName, string nameEn, string nameFa)
    {
        PackageName = packageName.Trim();
        NameEn = nameEn.Trim();
        NameFa = nameFa.Trim();
    }

    public void SetTier(bool isFree) => IsFree = isFree;
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
