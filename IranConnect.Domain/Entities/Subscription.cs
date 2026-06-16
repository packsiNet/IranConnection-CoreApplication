using IranConnect.Domain.Common;
using IranConnect.Domain.Enums;

namespace IranConnect.Domain.Entities;

public class Subscription : AuditableEntity
{
    public Guid UserId { get; private set; }
    public SubscriptionPlan Plan { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime ExpireDate { get; private set; }
    public bool IsAutoRenew { get; private set; }

    public bool IsActive => Status == SubscriptionStatus.Active
                         && ExpireDate > DateTime.UtcNow;

    public int DaysRemaining => IsActive
        ? (int)(ExpireDate - DateTime.UtcNow).TotalDays
        : 0;

    public User User { get; private set; } = default!;
    public ICollection<Payment> Payments { get; private set; }
        = new List<Payment>();

    private Subscription() { }

    public static Subscription CreateFree(Guid userId)
    {
        return new Subscription
        {
            UserId = userId,
            Plan = SubscriptionPlan.Free,
            Status = SubscriptionStatus.Active,
            StartDate = DateTime.UtcNow,
            ExpireDate = DateTime.UtcNow.AddYears(100)
        };
    }

    public static Subscription CreatePremium(Guid userId, int durationDays = 30)
    {
        return new Subscription
        {
            UserId = userId,
            Plan = SubscriptionPlan.Premium,
            Status = SubscriptionStatus.Active,
            StartDate = DateTime.UtcNow,
            ExpireDate = DateTime.UtcNow.AddDays(durationDays)
        };
    }

    public void Upgrade(int durationDays = 30)
    {
        Plan = SubscriptionPlan.Premium;
        Status = SubscriptionStatus.Active;
        StartDate = DateTime.UtcNow;
        ExpireDate = IsActive
            ? ExpireDate.AddDays(durationDays)
            : DateTime.UtcNow.AddDays(durationDays);
    }

    public void Cancel()
    {
        Status = SubscriptionStatus.Cancelled;
        IsAutoRenew = false;
    }

    public void Expire()
    {
        Status = SubscriptionStatus.Expired;
        Plan = SubscriptionPlan.Free;
    }
}
