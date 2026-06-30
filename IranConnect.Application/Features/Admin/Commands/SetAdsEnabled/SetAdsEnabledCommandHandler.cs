using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using IranConnect.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Admin.Commands.SetAdsEnabled;

public class SetAdsEnabledCommandHandler
    : IRequestHandler<SetAdsEnabledCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public SetAdsEnabledCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<bool>> Handle(
        SetAdsEnabledCommand request,
        CancellationToken cancellationToken)
    {
        var settings = await _context.AppSettings
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (settings is null)
        {
            settings = AppSettings.CreateDefault();
            _context.AppSettings.Add(settings);
        }

        settings.SetAdsEnabled(request.Enabled);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(settings.AdsEnabled);
    }
}
