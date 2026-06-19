using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Subscription.Queries.GetAppCatalog;

public record GetAppCatalogQuery() : IRequest<Result<List<AppCatalogResponse>>>;

public record AppCatalogResponse(
    string PackageName,
    string NameEn,
    string NameFa,
    bool IsFree);
