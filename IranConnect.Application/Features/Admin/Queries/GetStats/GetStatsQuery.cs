using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Admin.Queries.GetStats;

public record GetStatsQuery : IRequest<Result<StatsResponse>>;

public record StatsResponse(
    int TotalUsers,
    int ActiveUsers,
    int FreeUsers,
    int PremiumUsers,
    int ExpiredSubscriptions,
    int NewUsersToday,
    int NewUsersThisMonth);
