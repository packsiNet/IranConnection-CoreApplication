using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Admin.Queries.GetUserReceipts;

public class GetUserReceiptsAdminQueryHandler
    : IRequestHandler<GetUserReceiptsAdminQuery, Result<List<AdminReceiptResponse>>>
{
    private readonly IApplicationDbContext _context;

    public GetUserReceiptsAdminQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<List<AdminReceiptResponse>>> Handle(
        GetUserReceiptsAdminQuery request,
        CancellationToken cancellationToken)
    {
        var receipts = await _context.PaymentReceipts
            .Where(r => r.UserId == request.TargetUserId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new AdminReceiptResponse(
                r.Id.ToString(),
                r.PayerFullName,
                r.LastFourDigits,
                r.StoredFileName,
                r.OriginalFileName,
                r.Status.ToString(),
                r.RequestedDurationDays,
                r.CreatedAt,
                r.AdminNote,
                r.ReviewedAt))
            .ToListAsync(cancellationToken);

        return Result<List<AdminReceiptResponse>>.Success(receipts);
    }
}
