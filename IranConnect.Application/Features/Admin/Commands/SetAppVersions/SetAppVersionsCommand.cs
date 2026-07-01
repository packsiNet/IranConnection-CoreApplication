using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Admin.Commands.SetAppVersions;

public record SetAppVersionsCommand(
    string Version,
    string IranianAppsUpdateVersion) : IRequest<Result<AppVersionsResponse>>;

public record AppVersionsResponse(
    string Version,
    string IranianAppsUpdateVersion);
