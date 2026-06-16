using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.User.Commands.ResendVerification;

public record ResendVerificationCommand(Guid UserId)
    : IRequest<Result<string>>;
