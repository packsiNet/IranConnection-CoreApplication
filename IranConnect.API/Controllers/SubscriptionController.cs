using IranConnect.Application.Features.Subscription.Commands.RemoveAds;
using IranConnect.Application.Features.Subscription.Commands.UpgradeSubscription;
using IranConnect.Application.Features.Subscription.Queries.GetAppCatalog;
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

    /// <summary>کاتالوگ کامل اپ‌های پشتیبانی‌شده (هر اپ با پرچم isFree)</summary>
    [HttpGet("apps")]
    [ProducesResponseType(typeof(List<AppCatalogResponse>), 200)]
    public async Task<IActionResult> GetAppCatalog(
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new GetAppCatalogQuery(),
            cancellationToken);

        return HandleResult(result);
    }

    /// <summary>حذف تبلیغات برای کاربر جاری (پس از تأیید پرداخت)</summary>
    [HttpPost("remove-ads")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> RemoveAds(
        [FromQuery] Guid userId,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new RemoveAdsCommand(userId),
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
