using IranConnect.Domain.Common;
using IranConnect.Domain.Enums;

namespace IranConnect.Domain.Entities;

public class PaymentReceipt : BaseEntity
{
    public Guid UserId { get; private set; }
    public string PayerFullName { get; private set; } = default!;
    public string LastFourDigits { get; private set; } = default!;
    public string StoredFileName { get; private set; } = default!;
    public string OriginalFileName { get; private set; } = default!;
    public PaymentReceiptStatus Status { get; private set; }
    public PaymentReceiptType ReceiptType { get; private set; }
    public int RequestedDurationDays { get; private set; }
    public string? AdminNote { get; private set; }
    public Guid? ReviewedByAdminId { get; private set; }
    public DateTime? ReviewedAt { get; private set; }

    public User User { get; private set; } = default!;

    private PaymentReceipt() { }

    public static PaymentReceipt Create(
        Guid userId,
        string payerFullName,
        string lastFourDigits,
        string storedFileName,
        string originalFileName,
        int requestedDurationDays,
        PaymentReceiptType receiptType = PaymentReceiptType.PremiumUpgrade)
    {
        return new PaymentReceipt
        {
            UserId = userId,
            PayerFullName = payerFullName.Trim(),
            LastFourDigits = lastFourDigits,
            StoredFileName = storedFileName,
            OriginalFileName = originalFileName,
            RequestedDurationDays = requestedDurationDays,
            ReceiptType = receiptType,
            Status = PaymentReceiptStatus.Pending
        };
    }

    public void Approve(Guid adminId)
    {
        Status = PaymentReceiptStatus.Approved;
        ReviewedByAdminId = adminId;
        ReviewedAt = DateTime.UtcNow;
    }

    public void Reject(Guid adminId, string? note)
    {
        Status = PaymentReceiptStatus.Rejected;
        ReviewedByAdminId = adminId;
        ReviewedAt = DateTime.UtcNow;
        AdminNote = note;
    }
}
