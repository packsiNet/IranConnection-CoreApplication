using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using IranConnect.Application.Features.Admin.Queries.GetUsers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Admin.Queries.GetAllReviews;

public class GetAllReviewsQueryHandler
    : IRequestHandler<GetAllReviewsQuery, Result<PagedResult<AdminReviewItem>>>
{
    private readonly IApplicationDbContext _context;

    public GetAllReviewsQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<PagedResult<AdminReviewItem>>> Handle(
        GetAllReviewsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Reviews.AsQueryable();

        if (request.IsApproved.HasValue)
            query = query.Where(r => r.IsApproved == request.IsApproved.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new AdminReviewItem(
                r.Id,
                r.FullName,
                r.Rating,
                r.Comment,
                r.IpAddress,
                r.CountryCode,
                r.IsApproved,
                r.CreatedAt))
            .ToListAsync(cancellationToken);

        var result = new PagedResult<AdminReviewItem>(
            items,
            totalCount,
            request.Page,
            request.PageSize,
            (int)Math.Ceiling(totalCount / (double)request.PageSize));

        return Result<PagedResult<AdminReviewItem>>.Success(result);
    }
}
