using IranConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IranConnect.Infrastructure.Persistence.Configurations;

public class AppSettingsConfiguration : IEntityTypeConfiguration<AppSettings>
{
    public void Configure(EntityTypeBuilder<AppSettings> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.AdsEnabled).IsRequired();
        builder.Property(s => s.Version)
            .IsRequired().HasMaxLength(32).HasDefaultValue("1.0.0");
        builder.Property(s => s.IranianAppsUpdateVersion)
            .IsRequired().HasMaxLength(32).HasDefaultValue("1.0.0");
        builder.Property(s => s.UpdatedAt).IsRequired();
    }
}
