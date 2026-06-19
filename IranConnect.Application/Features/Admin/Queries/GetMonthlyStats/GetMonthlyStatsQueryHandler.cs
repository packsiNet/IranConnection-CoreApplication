using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using IranConnect.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Admin.Queries.GetMonthlyStats;

public class GetMonthlyStatsQueryHandler
    : IRequestHandler<GetMonthlyStatsQuery, Result<List<MonthlyStatItem>>>
{
    private readonly IApplicationDbContext _context;

    public GetMonthlyStatsQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<List<MonthlyStatItem>>> Handle(
        GetMonthlyStatsQuery request,
        CancellationToken cancellationToken)
    {
        var startDate = DateTime.UtcNow.AddMonths(-request.Months);

        var newUsers = await _context.Users
            .Where(u => u.CreatedAt >= startDate)
            .GroupBy(u => new { u.CreatedAt.Year, u.CreatedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var payments = await _context.PaymentReceipts
            .Where(r =>
                r.Status == PaymentReceiptStatus.Approved &&
                r.ReviewedAt >= startDate)
            .GroupBy(r => new { r.ReviewedAt!.Value.Year, r.ReviewedAt!.Value.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var result = Enumerable
            .Range(0, request.Months)
            .Select(i =>
            {
                var date = DateTime.UtcNow.AddMonths(-i);
                var year = date.Year;
                var month = date.Month;
                return new MonthlyStatItem(
                    year,
                    month,
                    date.ToString("MMMM yyyy"),
                    newUsers.FirstOrDefault(x => x.Year == year && x.Month == month)?.Count ?? 0,
                    payments.FirstOrDefault(x => x.Year == year && x.Month == month)?.Count ?? 0,
                    0);
            })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToList();

        return Result<List<MonthlyStatItem>>.Success(result);
    }
}
