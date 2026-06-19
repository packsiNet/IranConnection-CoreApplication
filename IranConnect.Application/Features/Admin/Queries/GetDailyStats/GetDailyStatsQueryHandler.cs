using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Admin.Queries.GetDailyStats;

public class GetDailyStatsQueryHandler
    : IRequestHandler<GetDailyStatsQuery, Result<List<DailyStatItem>>>
{
    private readonly IApplicationDbContext _context;

    public GetDailyStatsQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<List<DailyStatItem>>> Handle(
        GetDailyStatsQuery request,
        CancellationToken cancellationToken)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(-request.Days);

        var newUsers = await _context.Users
            .Where(u => u.CreatedAt >= startDate)
            .GroupBy(u => u.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var activeUsers = await _context.Users
            .Where(u => u.LastLoginAt >= startDate)
            .GroupBy(u => u.LastLoginAt!.Value.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var result = Enumerable
            .Range(0, request.Days)
            .Select(i =>
            {
                var date = DateTime.UtcNow.Date.AddDays(-i);
                return new DailyStatItem(
                    date,
                    newUsers.FirstOrDefault(x => x.Date == date)?.Count ?? 0,
                    activeUsers.FirstOrDefault(x => x.Date == date)?.Count ?? 0);
            })
            .OrderBy(x => x.Date)
            .ToList();

        return Result<List<DailyStatItem>>.Success(result);
    }
}
