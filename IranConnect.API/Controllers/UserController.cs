using IranConnect.API.Models.Requests;
using IranConnect.Application.Features.User.Commands.ResendVerification;
using IranConnect.Application.Features.User.Commands.UpdateProfile;
using IranConnect.Application.Features.User.Queries.GetProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IranConnect.API.Controllers;

[Authorize]
public class UserController : BaseController
{
    /// <summary>پروفایل کاربر جاری</summary>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(ProfileResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetProfile(
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var result = await Mediator.Send(
            new GetProfileQuery(userId.Value),
            cancellationToken);

        return HandleResult(result);
    }

    /// <summary>آپدیت پروفایل</summary>
    [HttpPut("profile")]
    [ProducesResponseType(typeof(ProfileResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var command = new UpdateProfileCommand(
            userId.Value,
            request.FullName,
            request.CurrentPassword,
            request.NewPassword);

        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>ارسال مجدد ایمیل تایید</summary>
    [HttpPost("resend-verification")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ResendVerification(
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var result = await Mediator.Send(
            new ResendVerificationCommand(userId.Value),
            cancellationToken);

        return HandleResult(result);
    }
}
