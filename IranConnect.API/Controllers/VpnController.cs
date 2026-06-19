using IranConnect.Application.Features.Vpn.Queries.GetVpnConfig;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IranConnect.API.Controllers;

[Authorize]
public class VpnController : BaseController
{
    /// <summary>دریافت کانفیگ WireGuard کاربر</summary>
    [HttpGet("config")]
    [ProducesResponseType(typeof(VpnConfigResponse), 200)]
    public async Task<IActionResult> GetConfig(
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var result = await Mediator.Send(
            new GetVpnConfigQuery(userId.Value),
            cancellationToken);

        return HandleResult(result);
    }
}
