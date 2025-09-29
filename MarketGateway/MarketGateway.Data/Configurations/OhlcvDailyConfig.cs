using MarketGateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketGateway.Data.Configurations;

public sealed class OhlcvDailyConfig : IEntityTypeConfiguration<OhlcvDailyEntity>
{
    public void Configure(EntityTypeBuilder<OhlcvDailyEntity> b)
    {
        b.ToTable("OhlcvDaily");
        b.HasKey(x => x.Id);


        b.Property(x => x.SeriesId).IsRequired();
        b.HasOne(x => x.Series)
            .WithMany()                    
            .HasForeignKey(x => x.SeriesId)
            .OnDelete(DeleteBehavior.Cascade);
        
        b.Property(x => x.TimestampUtc).IsRequired();
        b.HasIndex(x => new { x.SeriesId, x.TimestampUtc }).IsUnique();
        
        b.Property(x => x.Open) .HasPrecision(18, 6).IsRequired();
        b.Property(x => x.High) .HasPrecision(18, 6).IsRequired();
        b.Property(x => x.Low)  .HasPrecision(18, 6).IsRequired();
        b.Property(x => x.Close).HasPrecision(18, 6).IsRequired();

        b.Property(x => x.Volume);
    }
}