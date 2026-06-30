using IranConnect.Application.Common.Models;
using IranConnect.Domain.Enums;
using MediatR;

namespace IranConnect.Application.Features.Payment.Commands.SubmitReceipt;

public record SubmitPaymentReceiptCommand(
    Guid UserId,
    string PayerFullName,
    string LastFourDigits,
    byte[] FileBytes,
    string FileName,
    string ContentType,
    PaymentReceiptType ReceiptType = PaymentReceiptType.PremiumUpgrade,
    int DurationDays = 30
) : IRequest<Result<string>>;
