using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Admin.Commands.ActivateUser;

public record ActivateUserCommand(Guid TargetUserId)
    : IRequest<Result<string>>;
