namespace MarketGateway.Shared.DTOs;

public record QuoteResult : MarketDataResultBase
{
    public DateTime Timestamp { get; init; }
    public decimal? Price { get; init; }
    public decimal? Open { get; init; }
    public decimal? High { get; init; }
    public decimal? Low { get; init; }
    public decimal? PrevClose { get; init; }
    public decimal? Change { get; init; }
    public decimal? ChangePercent { get; init; }
    public decimal? Volume { get; init; }
}