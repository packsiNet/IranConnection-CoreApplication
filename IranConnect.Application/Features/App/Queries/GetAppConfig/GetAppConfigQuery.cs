using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.App.Queries.GetAppConfig;

// Public (no-auth) query for mobile clients: version + update markers + ads flag.
public record GetAppConfigQuery : IRequest<Result<AppConfigResponse>>;

public record AppConfigResponse(
    bool AdsEnabled,
    string Version,
    string IranianAppsUpdateVersion);
