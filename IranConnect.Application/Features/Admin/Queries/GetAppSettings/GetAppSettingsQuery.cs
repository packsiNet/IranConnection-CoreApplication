using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Admin.Queries.GetAppSettings;

public record GetAppSettingsQuery : IRequest<Result<AppSettingsResponse>>;

public record AppSettingsResponse(
    bool AdsEnabled,
    string Version,
    string IranianAppsUpdateVersion);
