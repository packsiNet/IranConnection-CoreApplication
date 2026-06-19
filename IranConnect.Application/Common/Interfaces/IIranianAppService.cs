namespace IranConnect.Application.Common.Interfaces;

public interface IIranianAppService
{
    /// <summary>کاتالوگ کامل اپ‌های پشتیبانی‌شده. هر اپ با پرچم IsFree.</summary>
    List<IranianAppDto> GetAppCatalog();
}

public record IranianAppDto(
    string PackageName,
    string NameEn,
    string NameFa,
    bool IsFree);
