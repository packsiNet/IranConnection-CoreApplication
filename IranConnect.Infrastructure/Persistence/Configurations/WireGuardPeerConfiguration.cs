using IranConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IranConnect.Infrastructure.Persistence.Configurations;

public class WireGuardPeerConfiguration
    : IEntityTypeConfiguration<WireGuardPeer>
{
    public void Configure(EntityTypeBuilder<WireGuardPeer> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.PublicKey)
            .IsRequired().HasMaxLength(256);
        builder.Property(p => p.PrivateKey)
            .IsRequired().HasMaxLength(256);
        builder.Property(p => p.AssignedIp)
            .IsRequired().HasMaxLength(32);
        builder.HasIndex(p => p.PublicKey).IsUnique();
        builder.HasIndex(p => p.AssignedIp).IsUnique();
        builder.HasIndex(p => p.UserId).IsUnique();
        builder.HasOne(p => p.User)
            .WithOne()
            .HasForeignKey<WireGuardPeer>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
