using IranConnect.Application.Common.Models;
using IranConnect.Application.Features.Admin.Queries.GetApps;
using MediatR;

namespace IranConnect.Application.Features.Admin.Commands.CreateApp;

public record CreateAppCommand(
    string PackageName,
    string NameEn,
    string NameFa,
    bool IsFree = false) : IRequest<Result<AdminAppResponse>>;
