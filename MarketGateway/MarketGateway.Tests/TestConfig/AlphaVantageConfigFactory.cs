using MarketGateway.Providers.Configuration;
using MarketGateway.Shared.DTOs;

namespace MarketGateway.Tests.TestConfig;

public static class AlphaVantageConfigFactory
{
    public static VendorConfig Build() => new VendorConfig
    {
        Vendor = "AlphaVantage",
        ApiKey = "demo",
        BaseUrl = "https://www.alphavantage.co/query",
        RateLimits = new RateLimitConfig { PerMinute = 5, PerHour = 0, PerDay = 25 },
        Endpoints = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Quote"] = new EndpointConfig
            {
                DataType = DataType.Quote,
                Function = "GLOBAL_QUOTE",
                HttpMethod = "GET",
                QueryParams = new QueryParamsConfig
                {
                    Required = new() { ["symbol"] = "{symbol}" },
                    Optional = new() { ["datatype"] = "{datatype}" }
                },
                Metadata = new EndpointMetadata
                {
                    TemporalResolutions = new() { "1min" },
                    Notes = "Simulates real-time quote using the latest bar."
                },
                Response = new ResponseConfig
                {
                    RootPath = "Global Quote",
                    TimestampKey = "07. latest trading day",
                    FieldMappings = new Dictionary<string, string>
                    {
                        ["Price"] = "05. price",
                        ["High"] = "03. high",
                        ["Low"] = "04. low",
                        ["Open"] = "02. open",
                        ["Volume"] = "06. volume",
                        ["PrevClose"] = "08. previous close"
                    }
                }
            }
        }
    };
}