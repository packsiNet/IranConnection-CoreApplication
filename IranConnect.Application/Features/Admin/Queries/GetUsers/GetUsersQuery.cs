using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Admin.Queries.GetUsers;

public record GetUsersQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    string? PlanFilter = null) : IRequest<Result<PagedResult<UserSummary>>>;

public record UserSummary(
    string Id,
    string Email,
    string? FullName,
    bool IsEmailVerified,
    bool IsActive,
    string Plan,
    string SubscriptionStatus,
    DateTime? ExpireDate,
    DateTime CreatedAt,
    DateTime? LastLoginAt);

public record PagedResult<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
