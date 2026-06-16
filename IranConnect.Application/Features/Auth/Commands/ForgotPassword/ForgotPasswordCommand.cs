using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Auth.Commands.ForgotPassword;

public record ForgotPasswordCommand(string Email) : IRequest<Result<string>>;
