using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Reviews.Queries.GetApprovedReviews;

public record GetApprovedReviewsQuery(
    int Page = 1,
    int PageSize = 10
) : IRequest<Result<ApprovedReviewsResponse>>;

public record ApprovedReviewsResponse(
    List<ApprovedReviewItem> Items,
    int TotalCount,
    double AverageRating
);

public record ApprovedReviewItem(
    Guid Id,
    string FullName,
    int Rating,
    string? Comment,
    string? CountryCode,
    DateTime CreatedAt
);
