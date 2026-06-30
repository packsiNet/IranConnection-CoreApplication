using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Admin.Commands.SetAdsEnabled;

public record SetAdsEnabledCommand(bool Enabled) : IRequest<Result<bool>>;
