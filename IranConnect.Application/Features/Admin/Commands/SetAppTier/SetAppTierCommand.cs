using IranConnect.Application.Common.Models;
using IranConnect.Application.Features.Admin.Queries.GetApps;
using MediatR;

namespace IranConnect.Application.Features.Admin.Commands.SetAppTier;

// Toggle Free (true) vs Premium (false) tier for an app.
public record SetAppTierCommand(Guid Id, bool IsFree)
    : IRequest<Result<AdminAppResponse>>;
