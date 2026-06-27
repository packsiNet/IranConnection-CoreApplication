using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using IranConnect.Application.Features.Admin.Queries.GetApps;
using IranConnect.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Admin.Commands.CreateApp;

public class CreateAppCommandHandler
    : IRequestHandler<CreateAppCommand, Result<AdminAppResponse>>
{
    private readonly IApplicationDbContext _context;

    public CreateAppCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<AdminAppResponse>> Handle(
        CreateAppCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.PackageName) ||
            string.IsNullOrWhiteSpace(request.NameEn) ||
            string.IsNullOrWhiteSpace(request.NameFa))
            return Result<AdminAppResponse>.Failure(
                "PackageName, NameEn و NameFa الزامی هستند", 400);

        var pkg = request.PackageName.Trim();
        var exists = await _context.IranianApps
            .AnyAsync(a => a.PackageName == pkg, cancellationToken);
        if (exists)
            return Result<AdminAppResponse>.Failure(
                "اپ با این PackageName قبلاً ثبت شده", 409);

        var app = IranianApp.Create(
            pkg, request.NameEn, request.NameFa, request.IsFree);
        _context.IranianApps.Add(app);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<AdminAppResponse>.Success(
            new AdminAppResponse(
                app.Id, app.PackageName, app.NameEn, app.NameFa,
                app.IsFree, app.IsActive, app.CreatedAt, app.UpdatedAt),
            201);
    }
}
