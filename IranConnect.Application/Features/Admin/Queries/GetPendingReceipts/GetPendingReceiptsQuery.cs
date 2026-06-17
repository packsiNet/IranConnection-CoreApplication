using IranConnect.Application.Common.Models;
using IranConnect.Application.Features.Admin.Queries.GetUsers;
using MediatR;

namespace IranConnect.Application.Features.Admin.Queries.GetPendingReceipts;

public record GetPendingReceiptsQuery(
    string? StatusFilter = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<PagedResult<ReceiptAdminResponse>>>;

public record ReceiptAdminResponse(
    Guid Id,
    string UserId,
    string UserEmail,
    string? UserFullName,
    string PayerFullName,
    string LastFourDigits,
    string StoredFileName,
    string OriginalFileName,
    string Status,
    int RequestedDurationDays,
    DateTime SubmittedAt,
    string? AdminNote,
    DateTime? ReviewedAt
);
