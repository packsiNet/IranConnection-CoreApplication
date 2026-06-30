using IranConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Subscription> Subscriptions { get; }
    DbSet<Payment> Payments { get; }
    DbSet<PaymentReceipt> PaymentReceipts { get; }
    DbSet<WireGuardPeer> WireGuardPeers { get; }
    DbSet<IranianApp> IranianApps { get; }
    DbSet<StatEvent> StatEvents { get; }
    DbSet<Review> Reviews { get; }
    DbSet<AppSettings> AppSettings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
