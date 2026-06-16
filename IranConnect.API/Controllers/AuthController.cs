using IranConnect.API.Models.Requests;
using IranConnect.Application.Features.Auth.Commands.ForgotPassword;
using IranConnect.Application.Features.Auth.Commands.Login;
using IranConnect.Application.Features.Auth.Commands.Logout;
using IranConnect.Application.Features.Auth.Commands.Register;
using IranConnect.Application.Features.Auth.Commands.RefreshToken;
using IranConnect.Application.Features.Auth.Commands.ResetPassword;
using IranConnect.Application.Features.Auth.Commands.VerifyEmail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace IranConnect.API.Controllers;

[AllowAnonymous]
public class AuthController : BaseController
{
    /// <summary>ثبت‌نام کاربر جدید</summary>
    [HttpPost("register")]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(RegisterResponse), 201)]
    [ProducesResponseType(409)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>ورود کاربر</summary>
    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var deviceInfo = HttpContext.Request.Headers["User-Agent"].ToString();

        var command = new LoginCommand(
            request.Email,
            request.Password,
            deviceInfo,
            ipAddress);

        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>تایید ایمیل</summary>
    [HttpGet("verify-email")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> VerifyEmail(
        [FromQuery] string email,
        [FromQuery] string token,
        CancellationToken cancellationToken)
    {
        var command = new VerifyEmailCommand(email, token);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>تجدید توکن</summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var deviceInfo = HttpContext.Request.Headers["User-Agent"].ToString();

        var command = new RefreshTokenCommand(
            request.Token,
            deviceInfo,
            ipAddress);

        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>فراموشی پسورد</summary>
    [HttpPost("forgot-password")]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>ریست پسورد</summary>
    [HttpPost("reset-password")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>خروج</summary>
    [HttpPost("logout")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LogoutCommand(request.RefreshToken);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}
