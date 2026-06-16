using IranConnect.API.Models.Requests;
using IranConnect.Application.Features.Admin.Commands.DeactivateUser;
using IranConnect.Application.Features.Admin.Queries.GetStats;
using IranConnect.Application.Features.Admin.Queries.GetUsers;
using IranConnect.Application.Features.Subscription.Commands.UpgradeSubscription;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IranConnect.API.Controllers;

[Authorize(Policy = "AdminOnly")]
public class AdminController : BaseController
{
    /// <summary>لیست کاربران</summary>
    [HttpGet("users")]
    [ProducesResponseType(typeof(PagedResult<UserSummary>), 200)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? plan = null,
        CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(
            new GetUsersQuery(page, pageSize, search, plan),
            cancellationToken);

        return HandleResult(result);
    }

    /// <summary>آمار کلی</summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(StatsResponse), 200)]
    public async Task<IActionResult> GetStats(
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new GetStatsQuery(),
            cancellationToken);

        return HandleResult(result);
    }

    /// <summary>ارتقا اشتراک کاربر</summary>
    [HttpPut("users/{userId}/upgrade")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpgradeUser(
        Guid userId,
        [FromBody] UpgradeUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new UpgradeSubscriptionCommand(userId, request.DurationDays),
            cancellationToken);

        return HandleResult(result);
    }

    /// <summary>غیرفعال کردن کاربر</summary>
    [HttpPut("users/{userId}/deactivate")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeactivateUser(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new DeactivateUserCommand(userId),
            cancellationToken);

        return HandleResult(result);
    }
}
