using System.Security.Claims;
using IranConnect.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IranConnect.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    private ISender? _mediator;
    protected ISender Mediator =>
        _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return StatusCode(result.StatusCode, result.Data);
        return StatusCode(result.StatusCode, new { error = result.Error });
    }

    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
            return StatusCode(result.StatusCode);
        return StatusCode(result.StatusCode, new { error = result.Error });
    }

    protected Guid? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }

    protected string? GetCurrentUserEmail()
        => User.FindFirst(ClaimTypes.Email)?.Value;

    protected string? GetCurrentUserPlan()
        => User.FindFirst("plan")?.Value;
}
