namespace MarketGateway.Shared.DTOs;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents a generic market data request that can describe any type of query
/// (prices, FX, fundamentals, indicators, macroeconomic data, etc.).
/// </summary>
public class MarketDataRequest
{
    /// <summary>
    /// Type of data requested (StockPrice, OptionChain, EconomicIndicator, etc.).
    /// Determines how the request is routed to providers.
    /// </summary>
    public DataType Type { get; set; }

    /// <summary>
    /// Primary identifier (e.g., ticker like "AAPL", FX pair like "EUR/USD").
    /// Can be null for requests like GDP or global indicators.
    /// </summary>
    public string? PrimaryIdentifier { get; set; }

    /// <summary>
    /// Start date (optional). Used for historical or ranged queries.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// End date (optional). If null, assume most recent data.
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Desired granularity (e.g., "1m", "5m", "daily", "weekly").
    /// Interpretation is vendor-specific, but helps routing.
    /// </summary>
    public string? Granularity { get; set; }

    /// <summary>
    /// Arbitrary parameters for vendor-specific or special fields
    /// (e.g., option strike, expiration, filters, country codes).
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();

    /// <summary>
    /// Optional: prioritize certain vendors (by name or priority weight).
    /// This can influence CollectorManager routing decisions.
    /// </summary>
    public List<string> PreferredVendors { get; set; } = new();
}