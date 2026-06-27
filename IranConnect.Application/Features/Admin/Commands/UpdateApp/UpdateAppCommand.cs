using IranConnect.Application.Common.Models;
using IranConnect.Application.Features.Admin.Queries.GetApps;
using MediatR;

namespace IranConnect.Application.Features.Admin.Commands.UpdateApp;

public record UpdateAppCommand(
    Guid Id,
    string PackageName,
    string NameEn,
    string NameFa) : IRequest<Result<AdminAppResponse>>;
