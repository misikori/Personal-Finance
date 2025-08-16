using System.Net;

namespace MarketGateway.Shared.DTOs;

/// <summary>
/// Normalized result of any market data fetch
/// Use to store in a DB or used downstream without vendor-specific quirks.
/// </summary>
public record MarketDataResultBase
{
    public string Vendor { get; set; } = string.Empty;
    public DataType Type { get; set; }
    public string? PrimaryIdentifier { get; set; }
    public string? RawJson { get; set; }
}

public sealed record APIResult<T> where T : MarketDataResultBase
{
    public required bool Success { get; init; }
    public T? Data { get; init; }
    public string? Error { get; init; }
    public HttpStatusCode StatusCode { get; init; }
    public DateTimeOffset? RetryAfter { get; init; }
    
}