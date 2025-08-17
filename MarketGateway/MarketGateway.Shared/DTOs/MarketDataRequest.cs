namespace MarketGateway.Shared.DTOs;

using System;
using System.Collections.Generic;

/// <summary>
/// Generic, proto-aligned request (adaptive across data types).
/// </summary>

public sealed class MarketDataRequest
{
    public DataType Type { get; set; }
    public List<IdentifierDto> Ids { get; init; } = new();
    public TimeRangeDto Range { get; init; } = new(null, null);
    public Dictionary<string, object?> Parameters { get; init; } = new();
    public List<string> PreferredVendors { get; init; } = new();

    // --- Back-compat helpers for existing provider code ---
    public string? PrimaryIdentifier
        => Ids.FirstOrDefault()?.Symbol
           ?? (Parameters.TryGetValue("symbol", out var s) ? s?.ToString() : null);

    public string? Granularity
        => Parameters.TryGetValue("granularity", out var g) ? g?.ToString() : null;
}