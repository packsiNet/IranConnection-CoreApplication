using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using IranConnect.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Admin.Queries.GetUsers;

public class GetUsersQueryHandler
    : IRequestHandler<GetUsersQuery, Result<PagedResult<UserSummary>>>
{
    private readonly IApplicationDbContext _context;

    public GetUsersQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<PagedResult<UserSummary>>> Handle(
        GetUsersQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Users
            .Include(u => u.Subscription)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(u =>
                u.Email.Contains(request.Search) ||
                (u.FullName != null &&
                 u.FullName.Contains(request.Search)));

        if (!string.IsNullOrWhiteSpace(request.PlanFilter) &&
            Enum.TryParse<SubscriptionPlan>(
                request.PlanFilter, true, out var plan))
            query = query.Where(u =>
                u.Subscription != null &&
                u.Subscription.Plan == plan);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new UserSummary(
                u.Id.ToString(),
                u.Email,
                u.FullName,
                u.IsEmailVerified,
                u.IsActive,
                u.Subscription != null
                    ? u.Subscription.Plan.ToString()
                    : "Free",
                u.Subscription != null
                    ? u.Subscription.Status.ToString()
                    : "Active",
                u.Subscription != null
                    ? u.Subscription.ExpireDate
                    : null,
                u.CreatedAt,
                u.LastLoginAt))
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(
            totalCount / (double)request.PageSize);

        return Result<PagedResult<UserSummary>>.Success(
            new PagedResult<UserSummary>(
                items, totalCount,
                request.Page, request.PageSize, totalPages));
    }
}
