using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Payment.Queries.GetMyReceipts;

public class GetMyReceiptsQueryHandler
    : IRequestHandler<GetMyReceiptsQuery, Result<List<MyReceiptResponse>>>
{
    private readonly IApplicationDbContext _context;

    public GetMyReceiptsQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<List<MyReceiptResponse>>> Handle(
        GetMyReceiptsQuery request,
        CancellationToken cancellationToken)
    {
        var receipts = await _context.PaymentReceipts
            .Where(r => r.UserId == request.UserId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new MyReceiptResponse(
                r.Id,
                r.PayerFullName,
                r.LastFourDigits,
                r.Status.ToString(),
                r.ReceiptType.ToString(),
                r.RequestedDurationDays,
                r.CreatedAt,
                r.AdminNote,
                r.ReviewedAt))
            .ToListAsync(cancellationToken);

        return Result<List<MyReceiptResponse>>.Success(receipts);
    }
}
