using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Subscription.Queries.GetAppCatalog;

public class GetAppCatalogQueryHandler
    : IRequestHandler<GetAppCatalogQuery, Result<List<AppCatalogResponse>>>
{
    private readonly IApplicationDbContext _context;

    public GetAppCatalogQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<List<AppCatalogResponse>>> Handle(
        GetAppCatalogQuery request,
        CancellationToken cancellationToken)
    {
        // Only active apps are exposed to clients; admins manage the catalog.
        var apps = await _context.IranianApps
            .Where(a => a.IsActive)
            .OrderBy(a => a.NameEn)
            .Select(a => new AppCatalogResponse(
                a.PackageName, a.NameEn, a.NameFa, a.IsFree))
            .ToListAsync(cancellationToken);

        return Result<List<AppCatalogResponse>>.Success(apps);
    }
}
