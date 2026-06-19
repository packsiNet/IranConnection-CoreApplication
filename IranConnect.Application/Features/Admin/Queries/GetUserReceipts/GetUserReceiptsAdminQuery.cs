using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Admin.Queries.GetUserReceipts;

public record GetUserReceiptsAdminQuery(Guid TargetUserId)
    : IRequest<Result<List<AdminReceiptResponse>>>;

public record AdminReceiptResponse(
    string Id,
    string PayerFullName,
    string LastFourDigits,
    string StoredFileName,
    string OriginalFileName,
    string Status,
    int RequestedDurationDays,
    DateTime SubmittedAt,
    string? AdminNote,
    DateTime? ReviewedAt);
