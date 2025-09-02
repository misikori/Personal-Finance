using System.ComponentModel.DataAnnotations;
using DataType = MarketGateway.Shared.DTOs.DataType;

namespace MarketGateway.Providers.Configuration;

public class VendorConfig
{
    [Required]
    public string Vendor { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    
    [Required, Url]
    public string BaseUrl { get; set; } = string.Empty;

    [Required]
    public RateLimitConfig RateLimits { get; set; } = new();

    /// <summary>
    /// Endpoints keyed by an arbitrary name (e.g., "GlobalQuote", "IntraDay").
    /// The key is case-insensitive; the value contains the endpoint definition.
    /// </summary>
    [Required]
    public Dictionary<string, EndpointConfig> Endpoints { get; set; }
        = new(StringComparer.OrdinalIgnoreCase);
    
    
    /// <summary>Returns true if any endpoint supports the given data type.</summary>
    public bool Supports(DataType type)
        => Endpoints.Values.Any(e => e.DataType == type);

    /// <summary>Returns endpoints for the given data type (often 1, but could be multiple).</summary>
    public IEnumerable<EndpointConfig> EndpointsFor(DataType type)
        => Endpoints.Values.Where(e => e.DataType == type);

    /// <summary>Validates obvious misconfigurations; throw or return false as you prefer.</summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Vendor))
            throw new ValidationException("Vendor name must be set.");
        if (string.IsNullOrWhiteSpace(BaseUrl))
            throw new ValidationException("BaseUrl must be set.");
        if (Endpoints.Count == 0)
            throw new ValidationException("At least one endpoint must be configured.");

        foreach (var (key, ep) in Endpoints)
        {
            if (ep.DataType == default)
                throw new ValidationException($"Endpoint '{key}' missing DataType.");
            if (string.IsNullOrWhiteSpace(ep.HttpMethod))
                throw new ValidationException($"Endpoint '{key}' missing HttpMethod.");
        }
    }
}

public class RateLimitConfig
{
    public int PerMinute { get; set; }
    public int PerHour { get; set; }
    public int PerDay { get; set; }
}

public class EndpointConfig
{
    [Required]
    public DataType DataType { get; set; }

    /// <summary>
    /// Optional vendor-specific function name (e.g., "GLOBAL_QUOTE" for AlphaVantage),
    /// used to build the URL/query.
    /// </summary>
    public string? Function { get; set; }

    /// <summary>HTTP verb to use; defaults to GET.</summary>
    public string HttpMethod { get; set; } = "GET";

    /// <summary>Optional path segment to append to BaseUrl (some vendors use path + query).</summary>
    public string? Path { get; set; }

    public QueryParamsConfig QueryParams { get; set; } = new();

    /// <summary>Non-functional metadata about the endpoint (e.g., intervals supported).</summary>
    public EndpointMetadata Metadata { get; set; } = new();

    /// <summary>Response parsing instructions.</summary>
    public ResponseConfig Response { get; set; } = new();
}

public class QueryParamsConfig
{
    // Case-insensitive keys to match YAML keys and avoid surprises.
    public Dictionary<string, string> Required { get; set; }
        = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, string> Optional { get; set; }
        = new(StringComparer.OrdinalIgnoreCase);
}

public class EndpointMetadata
{
    /// <summary>e.g., ["1m","5m","1d"] for candles. Keep as strings to match vendor terms.</summary>
    public List<string> TemporalResolutions { get; set; } = new();

    /// <summary>How far back the endpoint can go (e.g., 20 years for daily bars).</summary>
    public int HistoricalDepthYears { get; set; } = 0;

    public string? Notes { get; set; }
}

public class ResponseConfig
{
    /// <summary>
    /// Root JSON path (dot notation) to the payload; e.g., "Global Quote".
    /// Leave null for "root object".
    /// </summary>
    public string? RootPath { get; set; }

    /// <summary>
    /// Name of the field that represents the observation timestamp (if any).
    /// For date-only fields, parser should coerce to UTC midnight.
    /// </summary>
    public string? TimestampKey { get; set; }

    /// <summary>
    /// Map from DTO property names to JSON paths in the vendor payload.
    /// Example: "Price" -> "05. price"
    /// </summary>
    public Dictionary<string, string> FieldMappings { get; set; }
        = new(StringComparer.OrdinalIgnoreCase);
}