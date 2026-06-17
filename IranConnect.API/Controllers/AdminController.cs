using IranConnect.API.Models.Requests;
using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Features.Admin.Commands.DeactivateUser;
using IranConnect.Application.Features.Admin.Commands.ReviewReceipt;
using IranConnect.Application.Features.Admin.Queries.GetPendingReceipts;
using IranConnect.Application.Features.Admin.Queries.GetReceiptFile;
using IranConnect.Application.Features.Admin.Queries.GetStats;
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

    /// <summary>لیست رسیدهای پرداخت (پیش‌فرض: در انتظار بررسی)</summary>
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

    /// <summary>تأیید یا رد رسید پرداخت</summary>
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

    /// <summary>دانلود فایل رسید پرداخت</summary>
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
            return NotFound("فایل یافت نشد");

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
}
