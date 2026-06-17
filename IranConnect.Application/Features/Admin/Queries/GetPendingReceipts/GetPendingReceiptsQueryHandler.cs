using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using IranConnect.Application.Features.Admin.Queries.GetUsers;
using IranConnect.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Admin.Queries.GetPendingReceipts;

public class GetPendingReceiptsQueryHandler
    : IRequestHandler<GetPendingReceiptsQuery, Result<PagedResult<ReceiptAdminResponse>>>
{
    private readonly IApplicationDbContext _context;

    public GetPendingReceiptsQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<PagedResult<ReceiptAdminResponse>>> Handle(
        GetPendingReceiptsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.PaymentReceipts
            .Include(r => r.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.StatusFilter)
            && Enum.TryParse<PaymentReceiptStatus>(request.StatusFilter, true, out var status))
        {
            query = query.Where(r => r.Status == status);
        }
        else
        {
            query = query.Where(r => r.Status == PaymentReceiptStatus.Pending);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new ReceiptAdminResponse(
                r.Id,
                r.UserId.ToString(),
                r.User.Email,
                r.User.FullName,
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

        var result = new PagedResult<ReceiptAdminResponse>(
            items,
            totalCount,
            request.Page,
            request.PageSize,
            (int)Math.Ceiling(totalCount / (double)request.PageSize));

        return Result<PagedResult<ReceiptAdminResponse>>.Success(result);
    }
}
