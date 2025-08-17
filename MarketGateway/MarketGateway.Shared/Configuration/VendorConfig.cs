using System.ComponentModel.DataAnnotations;
using DataType = MarketGateway.Shared.DTOs.DataType;

namespace MarketGateway.Shared.Configuration;

public class VendorConfig
{
    [Required]
    public string Vendor { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    
    [Required, Url]
    public string BaseUrl { get; set; } = string.Empty;

    [Required]
    public RateLimitConfig RateLimits { get; set; } = new();
    [Required]
    public Dictionary<string, EndpointConfig> Endpoints { get; set; } =  new(StringComparer.OrdinalIgnoreCase);
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
    public DataType DataType {get;set;}
    public string? Function {get;set;}
    public string HttpMethod { get; set; } = "GET";
    public string? Path {get;set;}

    public QueryParamsConfig QueryParams { get; set; } = new();
    public EndpointMetadata Metadata { get; set; } = new();
    public ResponseConfig Response { get; set; } = new();
}

public class QueryParamsConfig
{
    public Dictionary<string, string> Required { get; set; } = new();
    public Dictionary<string, string> Optional { get; set; } = new();
}

public class EndpointMetadata
{
    public List<string> TemporalResolutions {get;set;} = new();
    public int HistoricalDepthYears { get; set; } = 0;
    public string? Notes { get; set; }
}

public class ResponseConfig
{
    public string? RootPath { get; set; }
    public string? TimestampKey { get; set; }
    public Dictionary<string,object> FieldMappings { get; set; } = new();
}