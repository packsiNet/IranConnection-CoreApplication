using IranConnect.API.Models.Requests;
using IranConnect.Application.Features.Payment.Commands.SubmitReceipt;
using IranConnect.Application.Features.Payment.Queries.GetMyReceipts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IranConnect.API.Controllers;

[Authorize]
public class PaymentController : BaseController
{
    /// <summary>ارسال رسید پرداخت کارت به کارت</summary>
    [HttpPost("receipt")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(429)]
    public async Task<IActionResult> SubmitReceipt(
        [FromForm] SubmitReceiptRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        if (request.ReceiptFile is null || request.ReceiptFile.Length == 0)
            return BadRequest("فایل رسید الزامی است");

        using var ms = new MemoryStream();
        await request.ReceiptFile.CopyToAsync(ms, cancellationToken);

        var result = await Mediator.Send(
            new SubmitPaymentReceiptCommand(
                userId.Value,
                request.PayerFullName,
                request.LastFourDigits,
                ms.ToArray(),
                request.ReceiptFile.FileName,
                request.ReceiptFile.ContentType,
                request.DurationDays),
            cancellationToken);

        return HandleResult(result);
    }

    /// <summary>لیست رسیدهای ارسال‌شده توسط کاربر جاری</summary>
    [HttpGet("receipts")]
    [ProducesResponseType(typeof(List<MyReceiptResponse>), 200)]
    public async Task<IActionResult> GetMyReceipts(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var result = await Mediator.Send(
            new GetMyReceiptsQuery(userId.Value),
            cancellationToken);

        return HandleResult(result);
    }
}
