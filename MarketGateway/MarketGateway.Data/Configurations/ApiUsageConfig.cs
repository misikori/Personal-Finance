using MarketGateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketGateway.Data.Configurations;


public class ApiUsageConfig : IEntityTypeConfiguration<ApiUsage>
{
    public void Configure(EntityTypeBuilder<ApiUsage> b)
    {
        b.ToTable("ApiUsages");
        b.HasIndex(x => new { x.Vendor, x.Date }).IsUnique();
        b.Property(x => x.Vendor).HasMaxLength(64).IsRequired();
    }
}