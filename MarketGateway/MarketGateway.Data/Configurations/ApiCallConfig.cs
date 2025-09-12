using MarketGateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketGateway.Data.Configurations;


public class ApiCallConfig : IEntityTypeConfiguration<ApiCall>
{
    public void Configure(EntityTypeBuilder<ApiCall> b)
    {
        b.ToTable("ApiCalls");
        b.HasIndex(x => new { x.Vendor, x.Identifier, x.RequestedAtUtc });

        b.Property(x => x.Vendor).HasMaxLength(64).IsRequired();
        b.Property(x => x.Identifier).HasMaxLength(128).IsRequired();
        b.Property(x => x.Error).HasMaxLength(4000);
    }
}