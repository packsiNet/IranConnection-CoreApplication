using IranConnect.Application.Common.Models;
using IranConnect.Application.Features.Auth.Commands.Login;
using MediatR;

namespace IranConnect.Application.Features.Auth.Commands.DeviceLogin;

public record DeviceLoginCommand(
    string DeviceId,
    string? DeviceInfo,
    string? IpAddress) : IRequest<Result<LoginResponse>>;
