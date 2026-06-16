using IranConnect.Application.Common.Models;
using IranConnect.Application.Features.User.Queries.GetProfile;
using MediatR;

namespace IranConnect.Application.Features.User.Commands.UpdateProfile;

public record UpdateProfileCommand(
    Guid UserId,
    string? FullName,
    string? CurrentPassword,
    string? NewPassword) : IRequest<Result<ProfileResponse>>;
