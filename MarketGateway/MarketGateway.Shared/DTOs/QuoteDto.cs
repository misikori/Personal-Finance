namespace MarketGateway.Shared.DTOs;

public sealed record QuoteDto : MarketDataResultBase
{
    // from YAML Response.FieldMappings keys -> same property names here
    public decimal? Price { get; set; }
    public decimal? Open { get; set; }
    public decimal? High { get; set; }
    public decimal? Low { get; set; }
    public decimal? PrevClose { get; set; }   // map if available
    public decimal? Volume { get; set; }

    public string? Currency { get; set; }
    public DateTime? Timestamp { get; set; }  // filled via TimestampKey, if configured
    // extra fields welcome, they just won’t be set by mapping if not configured
}
