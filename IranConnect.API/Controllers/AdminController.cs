using IranConnect.API.Models.Requests;
using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Features.Admin.Commands.ActivateUser;
using IranConnect.Application.Features.Admin.Commands.DeactivateUser;
using IranConnect.Application.Features.Admin.Commands.DeletePeer;
using IranConnect.Application.Features.Admin.Commands.DeleteUser;
using IranConnect.Application.Features.Admin.Commands.ResetPeer;
using IranConnect.Application.Features.Admin.Commands.ReviewReceipt;
using IranConnect.Application.Features.Admin.Commands.SetBandwidthLimit;
using IranConnect.Application.Features.Admin.Queries.GetBandwidthReport;
using IranConnect.Application.Features.Admin.Queries.GetDailyStats;
using IranConnect.Application.Features.Admin.Queries.GetMonthlyStats;
using IranConnect.Application.Features.Admin.Queries.GetOnlineStats;
using IranConnect.Application.Features.Admin.Queries.GetPeerDetail;
using IranConnect.Application.Features.Admin.Queries.GetPeerStats;
using IranConnect.Application.Features.Admin.Queries.GetPendingReceipts;
using IranConnect.Application.Features.Admin.Queries.GetReceiptFile;
using IranConnect.Application.Features.Admin.Queries.GetStats;
using IranConnect.Application.Features.Admin.Queries.GetUserBandwidth;
using IranConnect.Application.Features.Admin.Queries.GetUserDetail;
using IranConnect.Application.Features.Admin.Queries.GetUserReceipts;
using IranConnect.Application.Features.Admin.Queries.GetUsers;
using IranConnect.Application.Features.Subscription.Commands.UpgradeSubscription;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IranConnect.API.Controllers;

[Authorize(Policy = "AdminOnly")]
public class AdminController : BaseController
{
    private readonly IFileStorageService _fileStorage;

    public AdminController(IFileStorageService fileStorage)
        => _fileStorage = fileStorage;

    /// <summary>List users</summary>
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

    /// <summary>User detail</summary>
    [HttpGet("users/{userId}")]
    [ProducesResponseType(typeof(UserDetailResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetUserDetail(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new GetUserDetailQuery(userId), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>Overall statistics</summary>
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

    /// <summary>Daily statistics</summary>
    [HttpGet("stats/daily")]
    [ProducesResponseType(typeof(List<DailyStatItem>), 200)]
    public async Task<IActionResult> GetDailyStats(
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(
            new GetDailyStatsQuery(days), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>Monthly statistics</summary>
    [HttpGet("stats/monthly")]
    [ProducesResponseType(typeof(List<MonthlyStatItem>), 200)]
    public async Task<IActionResult> GetMonthlyStats(
        [FromQuery] int months = 12,
        CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(
            new GetMonthlyStatsQuery(months), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>Upgrade user subscription</summary>
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

    /// <summary>Deactivate user</summary>
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

    /// <summary>Reactivate user</summary>
    [HttpPut("users/{userId}/activate")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ActivateUser(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new ActivateUserCommand(userId), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>Delete user</summary>
    [HttpDelete("users/{userId}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteUser(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new DeleteUserCommand(userId), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>Payment receipts (default: pending review)</summary>
    [HttpGet("receipts")]
    [ProducesResponseType(typeof(PagedResult<ReceiptAdminResponse>), 200)]
    public async Task<IActionResult> GetReceipts(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(
            new GetPendingReceiptsQuery(status, page, pageSize),
            cancellationToken);

        return HandleResult(result);
    }

    /// <summary>Receipts for a specific user</summary>
    [HttpGet("users/{userId}/receipts")]
    [ProducesResponseType(typeof(List<AdminReceiptResponse>), 200)]
    public async Task<IActionResult> GetUserReceipts(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new GetUserReceiptsAdminQuery(userId), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>Approve or reject payment receipt</summary>
    [HttpPut("receipts/{receiptId}/review")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> ReviewReceipt(
        Guid receiptId,
        [FromBody] ReviewReceiptRequest request,
        CancellationToken cancellationToken)
    {
        var adminId = GetCurrentUserId();
        if (adminId is null) return Unauthorized();

        var result = await Mediator.Send(
            new ReviewReceiptCommand(receiptId, adminId.Value, request.Approved, request.Note),
            cancellationToken);

        return HandleResult(result);
    }

    /// <summary>Download payment receipt file</summary>
    [HttpGet("receipts/{receiptId}/file")]
    public async Task<IActionResult> GetReceiptFile(
        Guid receiptId,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new GetReceiptFileQuery(receiptId),
            cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });

        var fullPath = _fileStorage.GetFullPath(result.Data!.StoredFileName);
        if (!System.IO.File.Exists(fullPath))
            return NotFound("File not found");

        var ext = Path.GetExtension(result.Data.StoredFileName).ToLowerInvariant();
        var contentType = ext switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            _ => "image/jpeg"
        };

        var bytes = await System.IO.File.ReadAllBytesAsync(fullPath, cancellationToken);
        return File(bytes, contentType, result.Data.OriginalFileName);
    }

    /// <summary>All peers stats</summary>
    [HttpGet("vpn/peers")]
    [ProducesResponseType(typeof(List<PeerStatsResponse>), 200)]
    public async Task<IActionResult> GetPeerStats(
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new GetPeerStatsQuery(),
            cancellationToken);
        return HandleResult(result);
    }

    /// <summary>User peer detail</summary>
    [HttpGet("vpn/peers/{userId}")]
    [ProducesResponseType(typeof(PeerDetailResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetPeerDetail(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new GetPeerDetailQuery(userId), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>Delete user peer</summary>
    [HttpDelete("vpn/peers/{userId}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeletePeer(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new DeletePeerCommand(userId), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>Reset user peer</summary>
    [HttpPost("vpn/peers/{userId}/reset")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ResetPeer(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new ResetPeerCommand(userId), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>Set bandwidth limit</summary>
    [HttpPut("vpn/peers/{userId}/bandwidth-limit")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> SetBandwidthLimit(
        Guid userId,
        [FromBody] SetBandwidthLimitRequest request,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new SetBandwidthLimitCommand(userId, request.LimitBytes),
            cancellationToken);
        return HandleResult(result);
    }

    /// <summary>Bandwidth usage report for all users</summary>
    [HttpGet("vpn/bandwidth")]
    [ProducesResponseType(typeof(PagedResult<BandwidthReportItem>), 200)]
    public async Task<IActionResult> GetBandwidthReport(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = "total",
        CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(
            new GetBandwidthReportQuery(page, pageSize, sortBy),
            cancellationToken);
        return HandleResult(result);
    }

    /// <summary>User bandwidth usage</summary>
    [HttpGet("vpn/bandwidth/{userId}")]
    [ProducesResponseType(typeof(UserBandwidthResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetUserBandwidth(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new GetUserBandwidthQuery(userId), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>Concurrent online users</summary>
    [HttpGet("vpn/online")]
    [ProducesResponseType(typeof(OnlineStatsResponse), 200)]
    public async Task<IActionResult> GetOnlineStats(
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new GetOnlineStatsQuery(),
            cancellationToken);
        return HandleResult(result);
    }
}
