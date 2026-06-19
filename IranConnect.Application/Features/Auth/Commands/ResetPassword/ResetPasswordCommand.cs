using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Auth.Commands.ResetPassword;

public record ResetPasswordCommand(
    string Email,
    string Code,
    string NewPassword) : IRequest<Result<string>>;
