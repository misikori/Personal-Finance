namespace MarketGateway.Shared.DTOs;

public sealed class OhlcvBarDto
{
    public DateTimeOffset Ts { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public decimal? Volume { get; set; }
}

public sealed record OhlcvSeriesDto : MarketDataResultBase
{
    public string? Currency { get; set; }
    public BarGranularity Granularity { get; set; }
    public PriceAdjustment Adjustment { get; set; }
    public bool Partial { get; set; }
    public List<OhlcvBarDto> Bars { get; set; } = new();
}