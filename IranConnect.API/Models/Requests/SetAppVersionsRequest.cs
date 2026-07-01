namespace IranConnect.API.Models.Requests;

public record SetAppVersionsRequest(
    string Version,
    string IranianAppsUpdateVersion);
