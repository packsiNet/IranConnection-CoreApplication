using IranConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IranConnect.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Currency)
            .IsRequired().HasMaxLength(8);
        builder.Property(p => p.Status)
            .HasConversion<string>().HasMaxLength(32);
        builder.Property(p => p.GatewayRefId).HasMaxLength(128);
        builder.Property(p => p.GatewayTrackId).HasMaxLength(128);
        builder.Property(p => p.Authority).HasMaxLength(128);
        builder.Property(p => p.FailureReason).HasMaxLength(512);
    }
}
