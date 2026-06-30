using IranConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IranConnect.Infrastructure.Persistence.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.FullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(r => r.Comment)
            .HasMaxLength(1000);

        builder.Property(r => r.IpAddress)
            .HasMaxLength(45);

        builder.Property(r => r.CountryCode)
            .HasMaxLength(3);

        builder.HasIndex(r => r.IsApproved);
        builder.HasIndex(r => r.CreatedAt);
    }
}
