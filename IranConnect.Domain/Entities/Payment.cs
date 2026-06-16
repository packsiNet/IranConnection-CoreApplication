using IranConnect.Domain.Common;
using IranConnect.Domain.Enums;

namespace IranConnect.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid SubscriptionId { get; private set; }
    public long Amount { get; private set; }
    public string Currency { get; private set; } = "IRR";
    public PaymentStatus Status { get; private set; }
    public string? GatewayRefId { get; private set; }
    public string? GatewayTrackId { get; private set; }
    public string? Authority { get; private set; }
    public int DurationDays { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public string? FailureReason { get; private set; }

    public User User { get; private set; } = default!;
    public Subscription Subscription { get; private set; } = default!;

    private Payment() { }

    public static Payment Create(
        Guid userId,
        Guid subscriptionId,
        long amount,
        int durationDays,
        string? authority = null)
    {
        return new Payment
        {
            UserId = userId,
            SubscriptionId = subscriptionId,
            Amount = amount,
            DurationDays = durationDays,
            Status = PaymentStatus.Pending,
            Authority = authority
        };
    }

    public void MarkSuccess(string gatewayRefId, string? trackId = null)
    {
        Status = PaymentStatus.Success;
        GatewayRefId = gatewayRefId;
        GatewayTrackId = trackId;
        PaidAt = DateTime.UtcNow;
    }

    public void MarkFailed(string reason)
    {
        Status = PaymentStatus.Failed;
        FailureReason = reason;
    }
}
