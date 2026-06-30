using IranConnect.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace IranConnect.API.Models.Requests;

public class SubmitReceiptRequest
{
    public string PayerFullName { get; set; } = default!;
    public string LastFourDigits { get; set; } = default!;
    public IFormFile ReceiptFile { get; set; } = default!;
    public PaymentReceiptType ReceiptType { get; set; } = PaymentReceiptType.PremiumUpgrade;
    public int DurationDays { get; set; } = 30;
}
