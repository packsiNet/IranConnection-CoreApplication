using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Admin.Queries.GetReceiptFile;

public class GetReceiptFileQueryHandler
    : IRequestHandler<GetReceiptFileQuery, Result<ReceiptFileInfo>>
{
    private readonly IApplicationDbContext _context;

    public GetReceiptFileQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<ReceiptFileInfo>> Handle(
        GetReceiptFileQuery request,
        CancellationToken cancellationToken)
    {
        var receipt = await _context.PaymentReceipts
            .Where(r => r.Id == request.ReceiptId)
            .Select(r => new { r.StoredFileName, r.OriginalFileName })
            .FirstOrDefaultAsync(cancellationToken);

        if (receipt is null)
            return Result<ReceiptFileInfo>.Failure("رسید یافت نشد", 404);

        return Result<ReceiptFileInfo>.Success(
            new ReceiptFileInfo(receipt.StoredFileName, receipt.OriginalFileName));
    }
}
