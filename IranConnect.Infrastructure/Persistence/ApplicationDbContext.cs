using System.Reflection;
using IranConnect.Application.Common.Interfaces;
using IranConnect.Domain.Common;
using IranConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private static readonly Guid SeedAdminUserId =
        new("00000000-0000-0000-0000-000000000001");

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentReceipt> PaymentReceipts => Set<PaymentReceipt>();
    public DbSet<WireGuardPeer> WireGuardPeers => Set<WireGuardPeer>();
    public DbSet<IranianApp> IranianApps => Set<IranianApp>();
    public DbSet<StatEvent> StatEvents => Set<StatEvent>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<AppSettings> AppSettings => Set<AppSettings>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Seed values must stay constant across migrations: fixed Id/CreatedAt and a
        // precomputed PBKDF2 hash for password "Admin@12345" (PasswordHasher format).
        builder.Entity<User>().HasData(new
        {
            Id = SeedAdminUserId,
            Email = "admin@iranconnect.app",
            PasswordHash = "BHvJJbdOk6O62UodsWkO3A==.V/G+cCorxmIZKkC5CJ+IBm565AUOVzQAFfWbW6m2WDE=",
            IsEmailVerified = true,
            IsActive = true,
            IsAdmin = true,
            IsDeviceUser = false,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        builder.Entity<AppSettings>().HasData(new
        {
            Id = Domain.Entities.AppSettings.SingletonId,
            AdsEnabled = true,
            UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        base.OnModelCreating(builder);
    }

    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
        return await base.SaveChangesAsync(cancellationToken);
    }
}
