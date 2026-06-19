using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Subscription.Queries.GetAppCatalog;

public class GetAppCatalogQueryHandler
    : IRequestHandler<GetAppCatalogQuery, Result<List<AppCatalogResponse>>>
{
    private readonly IIranianAppService _iranianAppService;

    public GetAppCatalogQueryHandler(IIranianAppService iranianAppService)
    {
        _iranianAppService = iranianAppService;
    }

    public Task<Result<List<AppCatalogResponse>>> Handle(
        GetAppCatalogQuery request,
        CancellationToken cancellationToken)
    {
        var apps = _iranianAppService.GetAppCatalog()
            .Select(a => new AppCatalogResponse(
                a.PackageName,
                a.NameEn,
                a.NameFa,
                a.IsFree))
            .ToList();

        return Task.FromResult(
            Result<List<AppCatalogResponse>>.Success(apps));
    }
}
