using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Reviews.Queries.GetApprovedReviews;

public class GetApprovedReviewsQueryHandler
    : IRequestHandler<GetApprovedReviewsQuery, Result<ApprovedReviewsResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetApprovedReviewsQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<ApprovedReviewsResponse>> Handle(
        GetApprovedReviewsQuery request,
        CancellationToken cancellationToken)
    {
        var baseQuery = _context.Reviews.Where(r => r.IsApproved);

        var totalCount = await baseQuery.CountAsync(cancellationToken);
        var averageRating = totalCount > 0
            ? await baseQuery.AverageAsync(r => (double)r.Rating, cancellationToken)
            : 0;

        var items = await baseQuery
            .OrderByDescending(r => r.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new ApprovedReviewItem(
                r.Id,
                r.FullName,
                r.Rating,
                r.Comment,
                r.CountryCode,
                r.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result<ApprovedReviewsResponse>.Success(
            new ApprovedReviewsResponse(items, totalCount, Math.Round(averageRating, 1)));
    }
}
