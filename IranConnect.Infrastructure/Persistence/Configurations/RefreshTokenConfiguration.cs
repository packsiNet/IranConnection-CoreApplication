using IranConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IranConnect.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Token)
            .IsRequired().HasMaxLength(256);
        builder.HasIndex(r => r.Token).IsUnique();
        builder.Property(r => r.DeviceInfo).HasMaxLength(256);
        builder.Property(r => r.IpAddress).HasMaxLength(64);
        builder.Property(r => r.RevokedReason).HasMaxLength(256);
    }
}
