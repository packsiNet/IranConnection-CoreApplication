using IranConnect.Application.Common.Models;
using IranConnect.Application.Features.Auth.Commands.Login;
using MediatR;

namespace IranConnect.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(
    string Token,
    string? DeviceInfo,
    string? IpAddress) : IRequest<Result<LoginResponse>>;
