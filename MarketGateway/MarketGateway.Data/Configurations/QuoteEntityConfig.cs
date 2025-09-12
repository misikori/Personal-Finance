using MarketGateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketGateway.Data.Configurations;

public class QuoteEntityConfig : IEntityTypeConfiguration<QuoteEntity>
{
    public void Configure(EntityTypeBuilder<QuoteEntity> b)
    {
        b.ToTable("Quotes");
        b.HasIndex(x => new { x.Vendor, x.Ticker, x.TimestampUtc }).IsUnique();
        b.Property(x => x.Vendor).HasMaxLength(64).IsRequired();
        b.Property(x => x.Ticker).HasMaxLength(64).IsRequired();
        b.Property(x => x.Currency).HasMaxLength(16);
    }
}