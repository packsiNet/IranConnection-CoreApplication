using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Auth.Commands.VerifyEmail;

public record VerifyEmailCommand(
    string Email,
    string Token) : IRequest<Result<string>>;
