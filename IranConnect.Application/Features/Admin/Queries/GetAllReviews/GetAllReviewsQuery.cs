using IranConnect.Application.Common.Models;
using IranConnect.Application.Features.Admin.Queries.GetUsers;
using MediatR;

namespace IranConnect.Application.Features.Admin.Queries.GetAllReviews;

public record GetAllReviewsQuery(
    int Page = 1,
    int PageSize = 20,
    bool? IsApproved = null
) : IRequest<Result<PagedResult<AdminReviewItem>>>;

public record AdminReviewItem(
    Guid Id,
    string FullName,
    int Rating,
    string? Comment,
    string? IpAddress,
    string? CountryCode,
    bool IsApproved,
    DateTime CreatedAt
);
