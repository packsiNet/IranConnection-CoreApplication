using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Payment.Queries.GetMyReceipts;

public record GetMyReceiptsQuery(Guid UserId) : IRequest<Result<List<MyReceiptResponse>>>;

public record MyReceiptResponse(
    Guid Id,
    string PayerFullName,
    string LastFourDigits,
    string Status,
    int RequestedDurationDays,
    DateTime SubmittedAt,
    string? AdminNote,
    DateTime? ReviewedAt
);
