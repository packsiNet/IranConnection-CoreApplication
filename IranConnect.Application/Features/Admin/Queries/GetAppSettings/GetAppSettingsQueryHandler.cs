using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Admin.Queries.GetAppSettings;

public class GetAppSettingsQueryHandler
    : IRequestHandler<GetAppSettingsQuery, Result<AppSettingsResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetAppSettingsQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<AppSettingsResponse>> Handle(
        GetAppSettingsQuery request,
        CancellationToken cancellationToken)
    {
        var settings = await _context.AppSettings
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        var adsEnabled = settings?.AdsEnabled ?? true;
        return Result<AppSettingsResponse>.Success(new AppSettingsResponse(adsEnabled));
    }
}
