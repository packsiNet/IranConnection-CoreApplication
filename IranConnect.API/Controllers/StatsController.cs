using IranConnect.API.Models.Requests;
using IranConnect.Application.Features.Stats.Commands.RecordEvent;
using IranConnect.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IranConnect.API.Controllers;

[AllowAnonymous]
public class StatsController : BaseController
{
    /// <summary>ثبت ورود کاربر</summary>
    [HttpPost("login")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> RecordLogin(CancellationToken cancellationToken)
    {
        await Mediator.Send(
            new RecordEventCommand(StatEventType.Login, null, GetClientIpAddress()),
            cancellationToken);

        return NoContent();
    }

    /// <summary>ثبت کلیک دانلود</summary>
    [HttpPost("download-click")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> RecordDownloadClick(
        [FromBody] RecordDownloadClickRequest request,
        CancellationToken cancellationToken)
    {
        await Mediator.Send(
            new RecordEventCommand(StatEventType.DownloadClick, request.PackageName, GetClientIpAddress()),
            cancellationToken);

        return NoContent();
    }
}
