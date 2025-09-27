using MarketGateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketGateway.Data.Configurations;

public sealed class OhlcvSeriesConfig : IEntityTypeConfiguration<OhlcvSeriesEntity>
{
    public void Configure(EntityTypeBuilder<OhlcvSeriesEntity> b)
    {
        b.ToTable("OhlcvSeries");
        b.HasKey(x => x.Id);

        b.Property(x => x.Vendor).HasMaxLength(64).IsRequired();
        b.Property(x => x.Symbol).HasMaxLength(64).IsRequired();
        b.Property(x => x.Exchange).HasMaxLength(32);
        b.Property(x => x.Currency).HasMaxLength(16);
        
        b.HasIndex(x => new { x.Vendor, x.Symbol, x.Granularity, x.Adjustment })
            .IsUnique();

        b.Property(x => x.CreatedAtUtc).IsRequired();
    }
}