namespace IranConnect.API.Models.Requests;

public record CreateAppRequest(
    string PackageName,
    string NameEn,
    string NameFa,
    bool IsFree = false);

public record UpdateAppRequest(
    string PackageName,
    string NameEn,
    string NameFa);

public record SetAppTierRequest(bool IsFree);
