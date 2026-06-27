using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Admin.Commands.DeleteApp;

public class DeleteAppCommandHandler
    : IRequestHandler<DeleteAppCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public DeleteAppCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<string>> Handle(
        DeleteAppCommand request,
        CancellationToken cancellationToken)
    {
        var app = await _context.IranianApps
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
        if (app is null)
            return Result<string>.Failure("اپ پیدا نشد", 404);

        _context.IranianApps.Remove(app);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success($"اپ '{app.NameEn}' حذف شد");
    }
}
