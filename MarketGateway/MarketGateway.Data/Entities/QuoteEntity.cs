namespace MarketGateway.Data.Entities;

public class QuoteEntity
{
    public long Id { get; set; }

    public string Vendor { get; set; } = default!;
    public string Ticker { get; set; } = default!;
    public DateTime TimestampUtc { get; set; } 

    public decimal? Price { get; set; }
    public decimal? Open { get; set; }
    public decimal? High { get; set; }
    public decimal? Low { get; set; }
    public decimal? PrevClose { get; set; }
    public decimal? Volume { get; set; }   
    public string? Currency { get; set; }
}