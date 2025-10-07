using AutoMapper;
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MarketGateway.Contracts;
using MarketGateway.Grpc;
using MarketGateway.Interfaces;
using MarketGateway.Mapping;
using MarketGateway.Shared.API;
using MarketGateway.Shared.DTOs;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using DataType = MarketGateway.Contracts.DataType;

namespace MarketGateway.Tests.TestServices;

public class MarketDataGatewayServiceTests
{
    [Fact]
    public async Task Fetch_Quote_Maps_To_Proto_Success()
    {
        var broker = new Mock<IMarketDataBroker>();
        broker.Setup(b => b.FetchAsync(It.IsAny<MarketDataRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResult<MarketDataResultBase>.Ok(new QuoteDto
            {
                Vendor     = "AlphaVantage",
                Id = new IdentifierDto("IBM"),
                Price      = 123.45m,
                TimestampUtc  = DateTime.SpecifyKind(new DateTime(2099, 1, 1), DateTimeKind.Utc)
            }));

        var mapper = CreateMapper();
        var svc = new MarketDataGatewayService(broker.Object, NullLogger<MarketDataGatewayService>.Instance, mapper);

        var req = new FetchRequest
        {
            DataType = DataType.Quote,
            Parameters = new Struct(),
        };
        req.Ids.Add(new Identifier { Symbol = "IBM" });

        var res = await svc.Fetch(req, TestServerCallContext.Create());
        
        res.Ok.Should().BeTrue(res.Error);
        res.Quote.Should().NotBeNull();
        res.Quote.Id.Symbol.Should().Be("IBM");
        res.Quote.Price.Should().Be(123.45);
    }

    [Fact]
    public async Task Fetch_Unsupported_Result_Type_Returns_Error()
    {
        var broker = new Mock<IMarketDataBroker>();
        broker.Setup(b => b.FetchAsync(It.IsAny<MarketDataRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResult<MarketDataResultBase>.Ok(new UnknownResult()));

        var mapper = CreateMapper();
        var svc = new MarketDataGatewayService(broker.Object, NullLogger<MarketDataGatewayService>.Instance, mapper);

        var req = new FetchRequest { DataType = DataType.Quote };
        req.Ids.Add(new Identifier { Symbol = "IBM" });
        
        var res = await svc.Fetch(req, TestServerCallContext.Create());
        res.Ok.Should().BeFalse();
        res.Error.Should().Contain("Unsupported");
    }
    
    private static IMapper CreateMapper()
    {
        var cfg = new MapperConfiguration(c =>
        {
            c.AddProfile<ProtoMappingProfile>();
        });
        cfg.AssertConfigurationIsValid();
        return cfg.CreateMapper();
    }

    private sealed record UnknownResult : MarketDataResultBase { }

    private sealed class TestServerCallContext : ServerCallContext
    {
        public static ServerCallContext Create() => new TestServerCallContext();

        protected override string MethodCore => "marketdata.MarketDataGateway/Fetch";
        protected override string HostCore => "localhost";
        protected override string PeerCore => "ipv4:127.0.0.1:12345";
        protected override DateTime DeadlineCore => DateTime.UtcNow.AddMinutes(1);
        protected override Metadata RequestHeadersCore => new();
        protected override CancellationToken CancellationTokenCore => CancellationToken.None;
        protected override Metadata ResponseTrailersCore => new();
        protected override Status StatusCore { get; set; }
        protected override WriteOptions? WriteOptionsCore { get; set; }
        protected override AuthContext AuthContextCore => null!;
        protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions? options) => null!;
        protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders) => Task.CompletedTask;
    }
}
