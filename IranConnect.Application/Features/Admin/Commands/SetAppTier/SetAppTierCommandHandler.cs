using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using IranConnect.Application.Features.Admin.Queries.GetApps;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Admin.Commands.SetAppTier;

public class SetAppTierCommandHandler
    : IRequestHandler<SetAppTierCommand, Result<AdminAppResponse>>
{
    private readonly IApplicationDbContext _context;

    public SetAppTierCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<AdminAppResponse>> Handle(
        SetAppTierCommand request,
        CancellationToken cancellationToken)
    {
        var app = await _context.IranianApps
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
        if (app is null)
            return Result<AdminAppResponse>.Failure("اپ پیدا نشد", 404);

        app.SetTier(request.IsFree);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<AdminAppResponse>.Success(
            new AdminAppResponse(
                app.Id, app.PackageName, app.NameEn, app.NameFa,
                app.IsFree, app.IsActive, app.CreatedAt, app.UpdatedAt));
    }
}
