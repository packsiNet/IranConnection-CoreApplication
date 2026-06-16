using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Auth.Commands.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string? FullName) : IRequest<Result<RegisterResponse>>;

public record RegisterResponse(
    string UserId,
    string Email,
    string Message);
