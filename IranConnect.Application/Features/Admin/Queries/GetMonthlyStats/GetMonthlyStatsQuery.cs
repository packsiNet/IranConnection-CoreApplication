using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Admin.Queries.GetMonthlyStats;

public record GetMonthlyStatsQuery(int Months = 12)
    : IRequest<Result<List<MonthlyStatItem>>>;

public record MonthlyStatItem(
    int Year,
    int Month,
    string MonthName,
    int NewUsers,
    int ApprovedPayments,
    long TotalRevenue);
