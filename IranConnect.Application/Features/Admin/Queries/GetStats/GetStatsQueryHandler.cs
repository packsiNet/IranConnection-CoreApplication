using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using IranConnect.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Admin.Queries.GetStats;

public class GetStatsQueryHandler
    : IRequestHandler<GetStatsQuery, Result<StatsResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetStatsQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<StatsResponse>> Handle(
        GetStatsQuery request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var monthStart = new DateTime(now.Year, now.Month, 1);

        var totalUsers = await _context.Users
            .CountAsync(cancellationToken);

        var activeUsers = await _context.Users
            .CountAsync(u => u.IsActive, cancellationToken);

        var freeUsers = await _context.Subscriptions
            .CountAsync(
                s => s.Plan == SubscriptionPlan.Free && s.ShowAds,
                cancellationToken);

        var freeNoAdsUsers = await _context.Subscriptions
            .CountAsync(
                s => s.Plan == SubscriptionPlan.Free && !s.ShowAds,
                cancellationToken);

        var premiumUsers = await _context.Subscriptions
            .CountAsync(
                s => s.Plan == SubscriptionPlan.Premium &&
                     s.Status == SubscriptionStatus.Active &&
                     s.ExpireDate > now,
                cancellationToken);

        var expiredSubscriptions = await _context.Subscriptions
            .CountAsync(
                s => s.Status == SubscriptionStatus.Expired,
                cancellationToken);

        var newUsersToday = await _context.Users
            .CountAsync(
                u => u.CreatedAt >= todayStart,
                cancellationToken);

        var newUsersThisMonth = await _context.Users
            .CountAsync(
                u => u.CreatedAt >= monthStart,
                cancellationToken);

        return Result<StatsResponse>.Success(new StatsResponse(
            totalUsers,
            activeUsers,
            freeUsers,
            freeNoAdsUsers,
            premiumUsers,
            expiredSubscriptions,
            newUsersToday,
            newUsersThisMonth));
    }
}
