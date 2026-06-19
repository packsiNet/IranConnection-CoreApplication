using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Admin.Commands.DeleteUser;

public record DeleteUserCommand(Guid TargetUserId)
    : IRequest<Result<string>>;
