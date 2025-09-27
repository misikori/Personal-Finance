namespace MarketGateway.Shared.DTOs;

/// <summary>
/// Normalized result of any market data fetch
/// Use to store in a DB or used downstream without vendor-specific quirks.
/// </summary>
public record MarketDataResultBase
{
    public string Vendor { get; set; } = string.Empty;
    public DataType Type { get; set; }
    public IdentifierDto? Id { get; set; }
}

