using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Auth.Commands.VerifyEmail;

public record VerifyEmailCommand(
    string Email,
    string Code) : IRequest<Result<string>>;
