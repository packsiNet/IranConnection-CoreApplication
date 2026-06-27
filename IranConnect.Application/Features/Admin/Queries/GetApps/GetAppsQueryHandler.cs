using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Admin.Queries.GetApps;

public class GetAppsQueryHandler
    : IRequestHandler<GetAppsQuery, Result<List<AdminAppResponse>>>
{
    private readonly IApplicationDbContext _context;

    public GetAppsQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<List<AdminAppResponse>>> Handle(
        GetAppsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.IranianApps.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim().ToLower();
            query = query.Where(a =>
                a.NameEn.ToLower().Contains(s) ||
                a.NameFa.Contains(request.Search.Trim()) ||
                a.PackageName.ToLower().Contains(s));
        }

        if (request.IsFree.HasValue)
            query = query.Where(a => a.IsFree == request.IsFree.Value);

        if (request.IsActive.HasValue)
            query = query.Where(a => a.IsActive == request.IsActive.Value);

        var apps = await query
            .OrderBy(a => a.NameEn)
            .Select(a => new AdminAppResponse(
                a.Id, a.PackageName, a.NameEn, a.NameFa,
                a.IsFree, a.IsActive, a.CreatedAt, a.UpdatedAt))
            .ToListAsync(cancellationToken);

        return Result<List<AdminAppResponse>>.Success(apps);
    }
}
