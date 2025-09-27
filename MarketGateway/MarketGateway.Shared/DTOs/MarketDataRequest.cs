namespace MarketGateway.Shared.DTOs;
using System.Collections.Generic;

/// <summary>
/// Generic, proto-aligned request (adaptive across data types).
/// Only <see cref="Type"/> is required; everything else is optional and
/// interpreted per data type and per vendor.
/// </summary>
public sealed class MarketDataRequest
{
    public DataType Type { get; set; }

    /// <summary>Identifiers relevant to the request (e.g., symbol/exchange).</summary>
    public List<IdentifierDto> Ids { get; init; } = new();

    /// <summary>Optional time range for historical queries.</summary>
    public TimeRangeDto Range { get; init; } = new(null, null);

    /// <summary>Vendor-agnostic knobs (use typed helpers before this bag).</summary>
    public Dictionary<string, object?> Parameters { get; init; } = new();

    /// <summary>Preferred vendors (by name), highest priority first.</summary>
    public List<string> PreferredVendors { get; set; } = new();
    
    public string? GetSymbol() => Ids.Count > 0 ? Ids[0].Symbol : null;
    public void SetSymbol(string symbol, string? exchange = null) =>
        Ids.Insert(0, new IdentifierDto(symbol, exchange));
}