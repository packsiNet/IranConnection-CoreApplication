using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Admin.Queries.GetApps;

// Admin catalog listing — includes inactive apps. Optional filters.
public record GetAppsQuery(
    string? Search = null,
    bool? IsFree = null,
    bool? IsActive = null) : IRequest<Result<List<AdminAppResponse>>>;

public record AdminAppResponse(
    Guid Id,
    string PackageName,
    string NameEn,
    string NameFa,
    bool IsFree,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
