using System.Net;
using FluentAssertions;
using MarketGateway.Application;
using MarketGateway.Interfaces;
using MarketGateway.Providers.Interfaces;
using MarketGateway.Providers.Parsing;
using MarketGateway.Providers.Providers;
using MarketGateway.Shared.DTOs;
using MarketGateway.Tests.TestConfig;
using MarketGateway.Tests.TestHelpers;
using Microsoft.Extensions.Logging.Abstractions;

namespace MarketGateway.Tests.TestBroker;

public class MarketDataBrokerTests
{
    private static MarketDataRequest QuoteReq(string symbol = "IBM") => new()
    {
        Type = DataType.Quote,
        Ids = new() { new IdentifierDto(symbol) }
    };

    private static (IMarketDataProviderResolver, InMemoryStorageService) MakeResolver(
        (string vendor, string payload, HttpStatusCode status)[] vendors)
    {
        var httpFactory = new FakeHttpClientFactory();
        var storage     = new InMemoryStorageService(); 
        var parser      = new VendorResponseParser();
        var usage       = new InMemoryUsageTracker();
        var dict        = new Dictionary<string, IMarketDataProvider>(StringComparer.OrdinalIgnoreCase);

        foreach (var v in vendors)
        {
            var handler = new HttpMessageHandlerStub(_ => new HttpResponseMessage(v.status)
            {
                Content = new StringContent(v.payload)
            });

            var cfg = AlphaVantageConfigFactory.Build();
            cfg.Vendor  = v.vendor;
            cfg.ApiKey  = "demo";

            httpFactory.Add($"vendor:{v.vendor}", new HttpClient(handler));

            var provider = new VendorMarketDataProvider(
                cfg,
                httpFactory,
                usage,
                parser,
                NullLogger<VendorMarketDataProvider>.Instance
            );

            dict[v.vendor] = provider;
        }

        IMarketDataProviderResolver resolver = new MarketDataProviderResolver(dict);
        return (resolver, storage);
    }

    [Fact]
    public async Task StorageFirst_Returns_When_Fresh()
    {
        var (resolver, storage) = MakeResolver(new[]
        {
            ("AlphaVantage",
                @"{ ""Global Quote"": { ""01. symbol"": ""IBM"", ""05. price"": ""100.0"", ""07. latest trading day"": ""2099-01-01"" }}",
                HttpStatusCode.OK),
        });
        
        await storage.SaveApiResponseAsync("AlphaVantage", "IBM",
            @"{ ""Global Quote"": { ""01. symbol"": ""IBM"", ""05. price"": ""123.45"", ""07. latest trading day"": ""2099-01-01"" }}");

        var broker = new MarketDataBroker(resolver, storage, new VendorResponseParser(), NullLogger<MarketDataBroker>.Instance);
        var req = QuoteReq("IBM");

        var res = await broker.FetchAsync(req, default);

        res.Success.Should().BeTrue(res.Error);
        var dto = (QuoteDto)res.Data!;
        dto.Price.Should().Be(123.45m);
    }

    [Fact]
    public async Task Stale_Storage_Triggers_Live_Fetch()
    {
        var (resolver, storage) = MakeResolver(new[]
        {
            ("AlphaVantage",
                @"{ ""Global Quote"": { ""01. symbol"": ""IBM"", ""05. price"": ""111.0"", ""07. latest trading day"": ""2099-01-01"" }}",
                HttpStatusCode.OK),
        });

        // Pre-seed storage with old day (stale)
        await storage.SaveApiResponseAsync("AlphaVantage", "IBM",
            @"{ ""Global Quote"": { ""01. symbol"": ""IBM"", ""05. price"": ""99.99"", ""07. latest trading day"": ""2000-01-01"" }}");

        var broker = new MarketDataBroker(resolver, storage, new VendorResponseParser(), NullLogger<MarketDataBroker>.Instance);
        var req = QuoteReq("IBM");

        var res = await broker.FetchAsync(req, default);

        res.Success.Should().BeTrue(res.Error);
        var dto = (QuoteDto)res.Data!;
        dto.Price.Should().Be(111.0m); // from live fetch
    }

    [Fact]
    public async Task Preferred_Vendor_Is_Tried_First()
    {
        // v2 returns unique price so we can see ordering effect
        var (resolver, storage) = MakeResolver(new[]
        {
            ("V2", @"{ ""Global Quote"": { ""01. symbol"": ""IBM"", ""05. price"": ""222.22"", ""07. latest trading day"": ""2099-01-01"" }}", HttpStatusCode.OK),
            ("V1", @"{ ""Global Quote"": { ""01. symbol"": ""IBM"", ""05. price"": ""111.11"", ""07. latest trading day"": ""2099-01-01"" }}", HttpStatusCode.OK),
        });

        var broker = new MarketDataBroker(resolver, storage, new VendorResponseParser(), NullLogger<MarketDataBroker>.Instance);
        var req = QuoteReq("IBM");
        req.PreferredVendors = new() { "V2" };

        var res = await broker.FetchAsync(req, default);

        res.Success.Should().BeTrue(res.Error);
        ((QuoteDto)res.Data!).Price.Should().Be(222.22m);
    }

    [Fact]
    public async Task Aggregates_Errors_And_Handles_Failures()
    {
        var (resolver, storage) = MakeResolver(new[]
        {
            ("V1", @"{ ""note"": ""rate limit"" }", HttpStatusCode.TooManyRequests),
            ("V2", @"{ ""note"": ""rate limit"" }", HttpStatusCode.TooManyRequests),
        });

        var broker = new MarketDataBroker(resolver, storage, new VendorResponseParser(), NullLogger<MarketDataBroker>.Instance);
        var req = QuoteReq("IBM");

        var res = await broker.FetchAsync(req, default);

        res.Success.Should().BeFalse();
        res.Error.Should().Contain("V1").And.Contain("V2"); // aggregated messages
    }
}