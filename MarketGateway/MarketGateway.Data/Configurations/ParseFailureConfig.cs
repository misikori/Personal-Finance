using MarketGateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketGateway.Data.Configurations;

public class ParseFailureConfig : IEntityTypeConfiguration<ParseFailure>
{
    public void Configure(EntityTypeBuilder<ParseFailure> b)
    {
        b.ToTable("ParseFailures");
        b.HasIndex(x => new { x.Vendor, x.Type, x.PrimaryIdentifier, x.OccurredAtUtc });
        b.Property(x => x.Vendor).HasMaxLength(64).IsRequired();
        b.Property(x => x.PrimaryIdentifier).HasMaxLength(128).IsRequired();
        b.Property(x => x.Error).HasMaxLength(4000).IsRequired();
    }
}