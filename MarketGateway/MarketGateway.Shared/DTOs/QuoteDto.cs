namespace MarketGateway.Shared.DTOs;


public sealed record QuoteDto : MarketDataResultBase
{
    public decimal? Price { get; set; }
    public decimal? Open { get; set; }
    public decimal? High { get; set; }
    public decimal? Low { get; set; }
    public decimal? PrevClose { get; set; }
    public decimal? Volume { get; set; }

    public string? Currency { get; set; }

    /// <summary>Quote timestamp in UTC (if provided by vendor).</summary>
    public DateTimeOffset? TimestampUtc { get; set; }
}
