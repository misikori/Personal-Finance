using System.Net;
using System.Text;
using MarketGateway.Providers.Parsing;
using MarketGateway.Providers.Providers;
using MarketGateway.Tests.TestConfig;
using MarketGateway.Tests.TestHelpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public static class ProviderFactory
{
    public static VendorMarketDataProvider CreateAlphaVantageProvider(
        string json,
        Action<string>? assertUrl = null,
        HttpStatusCode status = HttpStatusCode.OK,
        int perDay = 0)
    {
        var handler = new HttpMessageHandlerStub(req =>
        {
            var url = req.RequestUri!.ToString();
            assertUrl?.Invoke(url);
            return new HttpResponseMessage(status)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        });

        var httpFactory = new FakeHttpClientFactory();
        httpFactory.Add("vendor:AlphaVantage", new HttpClient(handler));

        var cfg    = AlphaVantageConfigFactory.Build();
        var usage  = new InMemoryUsageTracker { PerDay = perDay };
        var parser = new VendorResponseParser();
        ILogger<VendorMarketDataProvider> logger = NullLogger<VendorMarketDataProvider>.Instance;
        
        return new VendorMarketDataProvider(cfg, httpFactory, usage, parser, logger);
    }
}