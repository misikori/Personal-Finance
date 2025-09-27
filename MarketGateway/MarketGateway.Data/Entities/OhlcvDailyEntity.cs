namespace MarketGateway.Data.Entities;

public class OhlcvDailyEntity {
    public long Id { get; set; }
    public int SeriesId { get; set; }
    public OhlcvSeriesEntity Series { get; set; } = default!;
    public DateTime TimestampUtc { get; set; }    
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low  { get; set; }
    public decimal Close { get; set; }
    public long? Volume { get; set; }
}