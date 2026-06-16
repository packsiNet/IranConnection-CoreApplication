using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Auth.Commands.ResetPassword;

public record ResetPasswordCommand(
    string Email,
    string Token,
    string NewPassword) : IRequest<Result<string>>;
