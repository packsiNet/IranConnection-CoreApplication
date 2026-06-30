using IranConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IranConnect.Infrastructure.Persistence.Configurations;

public class StatEventConfiguration : IEntityTypeConfiguration<StatEvent>
{
    public void Configure(EntityTypeBuilder<StatEvent> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.EventType)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(e => e.Metadata)
            .HasMaxLength(200);

        builder.Property(e => e.IpAddress)
            .HasMaxLength(45);

        builder.HasIndex(e => e.EventType);
        builder.HasIndex(e => e.CreatedAt);
    }
}
