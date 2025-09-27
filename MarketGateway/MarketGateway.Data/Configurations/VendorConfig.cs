using MarketGateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketGateway.Data.Configurations;

public class VendorConfig : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> b)
    {
        b.ToTable("Vendors");
        b.HasIndex(v => v.Name).IsUnique();
        b.Property(v => v.Name).HasMaxLength(64).IsRequired();
        b.Property(v => v.Notes).HasMaxLength(1000);
    }
}