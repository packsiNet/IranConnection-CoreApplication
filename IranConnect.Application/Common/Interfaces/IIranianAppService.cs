using IranConnect.Domain.Enums;

namespace IranConnect.Application.Common.Interfaces;

public interface IIranianAppService
{
    List<IranianAppDto> GetAllApps();
    List<IranianAppDto> GetAllowedApps(SubscriptionPlan plan);
    List<IranianAppDto> GetFreeApps();
}

public record IranianAppDto(
    string PackageName,
    string NameEn,
    string NameFa);
