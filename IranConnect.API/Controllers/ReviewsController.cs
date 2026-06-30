using IranConnect.API.Models.Requests;
using IranConnect.Application.Features.Reviews.Commands.SubmitReview;
using IranConnect.Application.Features.Reviews.Queries.GetApprovedReviews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IranConnect.API.Controllers;

[AllowAnonymous]
public class ReviewsController : BaseController
{
    /// <summary>ارسال نظر جدید</summary>
    [HttpPost]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> SubmitReview(
        [FromBody] SubmitReviewRequest request,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new SubmitReviewCommand(
                request.FullName,
                request.Rating,
                request.Comment,
                GetClientIpAddress()),
            cancellationToken);

        return HandleResult(result);
    }

    /// <summary>دریافت نظرات تأیید‌شده</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApprovedReviewsResponse), 200)]
    public async Task<IActionResult> GetReviews(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(
            new GetApprovedReviewsQuery(page, pageSize),
            cancellationToken);

        return HandleResult(result);
    }
}
