using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Auth.Commands.Login;

public record LoginCommand(
    string Email,
    string Password,
    string? DeviceInfo,
    string? IpAddress) : IRequest<Result<LoginResponse>>;

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    string Email,
    string? FullName,
    string Plan,
    bool ShowAds,
    bool IsEmailVerified);
