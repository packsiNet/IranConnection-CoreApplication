using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using IranConnect.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Admin.Commands.SetAppVersions;

public class SetAppVersionsCommandHandler
    : IRequestHandler<SetAppVersionsCommand, Result<AppVersionsResponse>>
{
    private readonly IApplicationDbContext _context;

    public SetAppVersionsCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<AppVersionsResponse>> Handle(
        SetAppVersionsCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Version) ||
            string.IsNullOrWhiteSpace(request.IranianAppsUpdateVersion))
            return Result<AppVersionsResponse>.Failure("مقدار نسخه نمی‌تواند خالی باشد", 400);

        var settings = await _context.AppSettings
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (settings is null)
        {
            settings = AppSettings.CreateDefault();
            _context.AppSettings.Add(settings);
        }

        settings.SetVersions(request.Version, request.IranianAppsUpdateVersion);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<AppVersionsResponse>.Success(
            new AppVersionsResponse(settings.Version, settings.IranianAppsUpdateVersion));
    }
}
