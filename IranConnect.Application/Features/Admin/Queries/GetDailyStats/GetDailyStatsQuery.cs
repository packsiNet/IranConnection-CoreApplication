using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Admin.Queries.GetDailyStats;

public record GetDailyStatsQuery(int Days = 30)
    : IRequest<Result<List<DailyStatItem>>>;

public record DailyStatItem(
    DateTime Date,
    int NewUsers,
    int ActiveUsers);
