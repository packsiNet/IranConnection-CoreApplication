using IranConnect.Domain.Entities;
using IranConnect.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IranConnect.Infrastructure.Persistence.Configurations;

public class PaymentReceiptConfiguration : IEntityTypeConfiguration<PaymentReceipt>
{
    public void Configure(EntityTypeBuilder<PaymentReceipt> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.PayerFullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.LastFourDigits)
            .IsRequired()
            .HasMaxLength(4);

        builder.Property(r => r.StoredFileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(r => r.OriginalFileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(r => r.AdminNote)
            .HasMaxLength(1000);

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.Status);
    }
}
