using IranConnect.Application.Features.App.Queries.GetAppConfig;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IranConnect.API.Controllers;

[AllowAnonymous]
public class AppController : BaseController
{
    /// <summary>پیکربندی عمومی اپ: نسخه، نسخه کاتالوگ و وضعیت تبلیغات (بدون احراز هویت — برای موبایل)</summary>
    [HttpGet("config")]
    [ProducesResponseType(typeof(AppConfigResponse), 200)]
    public async Task<IActionResult> GetConfig(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetAppConfigQuery(), cancellationToken);
        return HandleResult(result);
    }
}
