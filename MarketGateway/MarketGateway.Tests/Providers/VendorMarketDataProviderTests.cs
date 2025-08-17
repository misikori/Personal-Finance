using System.Net;
using System.Text;
using FluentAssertions;
using MarketGateway.Providers;
using MarketGateway.Shared.DTOs;
using MarketGateway.Tests.TestConfig;
using MarketGateway.Tests.TestHelpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public class VendorMarketDataProviderTests
{
    private static VendorMarketDataProvider CreateProvider(
        string json,
        Action<string>? assertUrl = null,
        HttpStatusCode status = HttpStatusCode.OK,
        int perMinute = 0, int perDay = 0)
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

        var cfg = AlphaVantageConfigFactory.Build();
        var usage = new InMemoryUsageTracker { PerMinute = perMinute, PerDay = perDay };
        var storage = new FileStorageServiceStub();
        ILogger<VendorMarketDataProvider> logger = NullLogger<VendorMarketDataProvider>.Instance;

        return new VendorMarketDataProvider(cfg, httpFactory, usage, storage, logger);
    }

    [Fact]
    public async Task GlobalQuote_HappyPath_Maps_Fields()
    {
        const string payload = @"{
          ""Global Quote"": {
            ""01. symbol"": ""IBM"",
            ""02. open"": ""160.5000"",
            ""03. high"": ""162.0000"",
            ""04. low"": ""159.8000"",
            ""05. price"": ""161.2000"",
            ""06. volume"": ""3456789"",
            ""07. latest trading day"": ""2025-08-15"",
            ""08. previous close"": ""160.9000""
          }
        }";

        var provider = CreateProvider(payload, assertUrl: url =>
        {
            url.Should().Contain("function=GLOBAL_QUOTE");
            url.Should().Contain("symbol=IBM");
            url.Should().Contain("apikey=demo");
        });

        var req = new MarketDataRequest
        {
            Type = DataType.Quote,
            Ids = new() { new IdentifierDto("IBM") },
            Parameters = new() { ["datatype"] = "json" }
        };

        var res = await provider.FetchAsync(req, default);

        res.Success.Should().BeTrue(res.Error);
        res.Data.Should().BeOfType<QuoteDto>();
        var dto = (QuoteDto)res.Data!;
        dto.Vendor.Should().Be("AlphaVantage");
        dto.PrimaryIdentifier.Should().Be("IBM");

        dto.Price.Should().Be(161.2000m);
        dto.High.Should().Be(162.0000m);
        dto.Low.Should().Be(159.8000m);
        dto.Open.Should().Be(160.5000m);
        dto.Volume.Should().Be(3456789m);

        dto.Timestamp.Should().Be(new DateTime(2025, 8, 15, 2, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public async Task RateLimit_Blocks_Request_With_RetryAfter()
    {
        var provider = CreateProvider("{}", perMinute: 5, perDay: 25);

        var req = new MarketDataRequest
        {
            Type = DataType.Quote,
            Ids = new() { new IdentifierDto("IBM") }
        };

        var (allowed, reason, retry) = await provider.CanFetchDataAsync(req, default);
        allowed.Should().BeFalse();
        reason.Should().Contain("Rate limit");
        retry.Should().NotBeNull();

        var res = await provider.FetchAsync(req, default);
        res.Success.Should().BeFalse();
        res.Error.Should().Contain("Rate limit");
        res.RetryAfter.Should().NotBeNull();
    }

    [Fact]
    public async Task HttpError_Is_Propagated()
    {
        const string payload = @"{ ""note"": ""thank you for using demo key"" }";
        var provider = CreateProvider(payload, status: HttpStatusCode.TooManyRequests);

        var req = new MarketDataRequest
        {
            Type = DataType.Quote,
            Ids = new() { new IdentifierDto("IBM") }
        };

        var res = await provider.FetchAsync(req, default);

        res.Success.Should().BeFalse();
        res.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        res.Error.Should().Contain("HTTP").And.Contain("429");
    }

    [Fact]
    public async Task MissingFields_Result_In_Nulls_Not_Fail()
    {
        const string payload = @"{ ""Global Quote"": { ""01. symbol"": ""IBM"" } }";
        var provider = CreateProvider(payload);

        var req = new MarketDataRequest
        {
            Type = DataType.Quote,
            Ids = new() { new IdentifierDto("IBM") }
        };

        var res = await provider.FetchAsync(req, default);

        res.Success.Should().BeTrue(res.Error);
        var dto = (QuoteDto)res.Data!;
        dto.Price.Should().BeNull();
        dto.Open.Should().BeNull();
        dto.High.Should().BeNull();
        dto.Low.Should().BeNull();
        dto.Volume.Should().BeNull();
        dto.Timestamp.Should().BeNull();
    }

    [Fact]
    public async Task Unsupported_DataType_Returns_Fail()
    {
        const string payload = @"{}";
        var provider = CreateProvider(payload);

        var req = new MarketDataRequest
        {
            Type = (DataType)999,
            Ids = new() { new IdentifierDto("IBM") }
        };

        var res = await provider.FetchAsync(req, default);

        res.Success.Should().BeFalse();
        res.Error.Should().Contain("not supported");
    }
}
