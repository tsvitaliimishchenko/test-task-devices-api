using DevicesApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevicesApi.Infrastructure.Persistence.Configurations;

public class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable("Devices");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();

        builder.Property(d => d.Name).IsRequired().HasMaxLength(200);
        builder.Property(d => d.Brand).IsRequired().HasMaxLength(200);
        builder.Property(d => d.State).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(d => d.CreationTime).IsRequired();

        builder.HasIndex(d => d.Brand);
        builder.HasIndex(d => d.State);
    }
}
