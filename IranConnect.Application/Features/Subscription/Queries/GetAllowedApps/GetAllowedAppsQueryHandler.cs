using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using IranConnect.Application.Features.Subscription.Queries.GetSubscription;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Subscription.Queries.GetAllowedApps;

public class GetAllowedAppsQueryHandler
    : IRequestHandler<GetAllowedAppsQuery, Result<List<AllowedAppResponse>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIranianAppService _iranianAppService;

    public GetAllowedAppsQueryHandler(
        IApplicationDbContext context,
        IIranianAppService iranianAppService)
    {
        _context = context;
        _iranianAppService = iranianAppService;
    }

    public async Task<Result<List<AllowedAppResponse>>> Handle(
        GetAllowedAppsQuery request,
        CancellationToken cancellationToken)
    {
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(
                s => s.UserId == request.UserId,
                cancellationToken);

        if (subscription is null)
            return Result<List<AllowedAppResponse>>
                .Failure("اشتراک یافت نشد", 404);

        var apps = _iranianAppService.GetAllowedApps(subscription.Plan);

        return Result<List<AllowedAppResponse>>.Success(
            apps.Select(a => new AllowedAppResponse(
                a.PackageName,
                a.NameEn,
                a.NameFa)).ToList());
    }
}
