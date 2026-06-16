using IranConnect.Application.Features.Subscription.Commands.UpgradeSubscription;
using IranConnect.Application.Features.Subscription.Queries.GetAllowedApps;
using IranConnect.Application.Features.Subscription.Queries.GetSubscription;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IranConnect.API.Controllers;

[Authorize]
public class SubscriptionController : BaseController
{
    /// <summary>وضعیت اشتراک کاربر جاری</summary>
    [HttpGet]
    [ProducesResponseType(typeof(SubscriptionResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetSubscription(
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var result = await Mediator.Send(
            new GetSubscriptionQuery(userId.Value),
            cancellationToken);

        return HandleResult(result);
    }

    /// <summary>لیست اپ‌های مجاز بر اساس پلن</summary>
    [HttpGet("apps")]
    [ProducesResponseType(typeof(List<AllowedAppResponse>), 200)]
    public async Task<IActionResult> GetAllowedApps(
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var result = await Mediator.Send(
            new GetAllowedAppsQuery(userId.Value),
            cancellationToken);

        return HandleResult(result);
    }

    /// <summary>ارتقا اشتراک — فقط ادمین</summary>
    [HttpPost("upgrade")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(SubscriptionResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Upgrade(
        [FromBody] UpgradeSubscriptionCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}
