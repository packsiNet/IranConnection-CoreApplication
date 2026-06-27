using IranConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IranConnect.Infrastructure.Persistence.Configurations;

public class IranianAppConfiguration : IEntityTypeConfiguration<IranianApp>
{
    public void Configure(EntityTypeBuilder<IranianApp> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.PackageName)
            .IsRequired().HasMaxLength(256);
        builder.Property(a => a.NameEn)
            .IsRequired().HasMaxLength(128);
        builder.Property(a => a.NameFa)
            .IsRequired().HasMaxLength(128);
        builder.HasIndex(a => a.PackageName).IsUnique();
    }
}
