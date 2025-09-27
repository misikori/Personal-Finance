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
        Ids = [new IdentifierDto(symbol)]
    };

    private static (IMarketDataProviderResolver, InMemoryStorageService) MakeResolver(
        (string vendor, string payload, HttpStatusCode status)[] vendors)
    {
        var httpFactory = new FakeHttpClientFactory();
        var storage     = new InMemoryStorageService(); 
        var parser      = new VendorResponseParser();
        var usage       = new InMemoryUsageTracker();
        var providers   = new List<IMarketDataProvider>();

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

            providers.Add(provider);
        }

        IMarketDataProviderResolver resolver = new MarketDataProviderResolver(providers, NullLogger<MarketDataProviderResolver>.Instance);
        return (resolver, storage);
    }
    

    [Fact]
    public async Task StorageFirst_Returns_When_Fresh()
    {
        var (resolver, storage) = MakeResolver([
            ("AlphaVantage",
                @"{ ""Global Quote"": { ""01. symbol"": ""IBM"", ""05. price"": ""100.0"", ""07. latest trading day"": ""2099-01-01"" }}",
                HttpStatusCode.OK)
        ]);

        // Seed a fresh parsed quote (within 24h)
        storage.SeedParsedQuote("AlphaVantage", "IBM", 123.45m, DateTimeOffset.UtcNow);

        var broker = new MarketDataBroker(resolver, storage, new VendorResponseParser(), NullLogger<MarketDataBroker>.Instance);
        var req = new MarketDataRequest { Type = DataType.Quote, Ids = [new IdentifierDto("IBM")] };

        var res = await broker.FetchAsync(req, CancellationToken.None);

        res.Success.Should().BeTrue(res.Error);
        var dto = (QuoteDto)res.Data!;
        dto.Price.Should().Be(123.45m);
    }

    [Fact]
    public async Task Stale_Storage_Triggers_Live_Fetch()
    {
        var (resolver, storage) = MakeResolver([
            ("AlphaVantage",
                @"{ ""Global Quote"": { ""01. symbol"": ""IBM"", ""05. price"": ""111.0"", ""07. latest trading day"": ""2099-01-01"" }}",
                HttpStatusCode.OK)
        ]);
        
        storage.SeedParsedQuote("AlphaVantage", "IBM", 99.99m, new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero));

        var broker = new MarketDataBroker(resolver, storage, new VendorResponseParser(), NullLogger<MarketDataBroker>.Instance);
        var req = new MarketDataRequest { Type = DataType.Quote, Ids = [new IdentifierDto("IBM")] };

        var res = await broker.FetchAsync(req, CancellationToken.None);

        res.Success.Should().BeTrue(res.Error);
        var dto = (QuoteDto)res.Data!;
        dto.Price.Should().Be(111.0m);
    }

    [Fact]
    public async Task Preferred_Vendor_Is_Tried_First()
    {
        // v2 returns unique price so we can see ordering effect
        var (resolver, storage) = MakeResolver([
            ("V2", @"{ ""Global Quote"": { ""01. symbol"": ""IBM"", ""05. price"": ""222.22"", ""07. latest trading day"": ""2099-01-01"" }}", HttpStatusCode.OK),
            ("V1", @"{ ""Global Quote"": { ""01. symbol"": ""IBM"", ""05. price"": ""111.11"", ""07. latest trading day"": ""2099-01-01"" }}", HttpStatusCode.OK)
        ]);

        var broker = new MarketDataBroker(resolver, storage, new VendorResponseParser(), NullLogger<MarketDataBroker>.Instance);
        var req = QuoteReq();
        req.PreferredVendors = ["V2"];

        var res = await broker.FetchAsync(req, CancellationToken.None);

        res.Success.Should().BeTrue(res.Error);
        ((QuoteDto)res.Data!).Price.Should().Be(222.22m);
    }

    [Fact]
    public async Task Aggregates_Errors_And_Handles_Failures()
    {
        var (resolver, storage) = MakeResolver([
            ("V1", @"{ ""note"": ""rate limit"" }", HttpStatusCode.TooManyRequests),
            ("V2", @"{ ""note"": ""rate limit"" }", HttpStatusCode.TooManyRequests)
        ]);

        var broker = new MarketDataBroker(resolver, storage, new VendorResponseParser(), NullLogger<MarketDataBroker>.Instance);
        var req = QuoteReq();

        var res = await broker.FetchAsync(req, CancellationToken.None);

        res.Success.Should().BeFalse();
        res.Error.Should().Contain("V1").And.Contain("V2"); // aggregated messages
    }
}