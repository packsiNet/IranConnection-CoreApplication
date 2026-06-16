using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Admin.Commands.DeactivateUser;

public record DeactivateUserCommand(Guid TargetUserId)
    : IRequest<Result<string>>;
