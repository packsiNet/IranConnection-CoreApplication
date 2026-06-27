using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using IranConnect.Application.Features.Admin.Queries.GetApps;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Admin.Commands.UpdateApp;

public class UpdateAppCommandHandler
    : IRequestHandler<UpdateAppCommand, Result<AdminAppResponse>>
{
    private readonly IApplicationDbContext _context;

    public UpdateAppCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<AdminAppResponse>> Handle(
        UpdateAppCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.PackageName) ||
            string.IsNullOrWhiteSpace(request.NameEn) ||
            string.IsNullOrWhiteSpace(request.NameFa))
            return Result<AdminAppResponse>.Failure(
                "PackageName, NameEn و NameFa الزامی هستند", 400);

        var app = await _context.IranianApps
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
        if (app is null)
            return Result<AdminAppResponse>.Failure("اپ پیدا نشد", 404);

        var pkg = request.PackageName.Trim();
        var clash = await _context.IranianApps.AnyAsync(
            a => a.PackageName == pkg && a.Id != request.Id, cancellationToken);
        if (clash)
            return Result<AdminAppResponse>.Failure(
                "PackageName برای اپ دیگری ثبت شده", 409);

        app.Update(pkg, request.NameEn, request.NameFa);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<AdminAppResponse>.Success(
            new AdminAppResponse(
                app.Id, app.PackageName, app.NameEn, app.NameFa,
                app.IsFree, app.IsActive, app.CreatedAt, app.UpdatedAt));
    }
}
