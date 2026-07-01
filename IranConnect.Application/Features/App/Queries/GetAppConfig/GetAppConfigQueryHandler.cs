using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.App.Queries.GetAppConfig;

public class GetAppConfigQueryHandler
    : IRequestHandler<GetAppConfigQuery, Result<AppConfigResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetAppConfigQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<AppConfigResponse>> Handle(
        GetAppConfigQuery request,
        CancellationToken cancellationToken)
    {
        var settings = await _context.AppSettings
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        var response = new AppConfigResponse(
            settings?.AdsEnabled ?? true,
            settings?.Version ?? "1.0.0",
            settings?.IranianAppsUpdateVersion ?? "1.0.0");
        return Result<AppConfigResponse>.Success(response);
    }
}
