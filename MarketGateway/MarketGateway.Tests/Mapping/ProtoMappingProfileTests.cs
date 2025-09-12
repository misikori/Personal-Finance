using AutoMapper;
using FluentAssertions;
using MarketGateway.Contracts;
using MarketGateway.Mapping;

namespace MarketGateway.Tests.Mapping;
using MarketGateway.Shared.DTOs;

public class ProtoMappingProfileTests
{
    private readonly IMapper _mapper;

    public ProtoMappingProfileTests()
    {
        var cfg = new MapperConfiguration(c => c.AddProfile<ProtoMappingProfile>());
        cfg.AssertConfigurationIsValid();
        _mapper = cfg.CreateMapper();
    }

    [Fact]
    public void QuoteDto_Maps_To_Quote()
    {
        var dto = new QuoteDto
        {
            Vendor = "AlphaVantage",
            Id = new IdentifierDto("IBM"),    
            Price = 123.45m,
            Currency = "USD",        
            TimestampUtc = DateTime.SpecifyKind(new DateTime(2099,1,1,12,0,0), DateTimeKind.Utc)
        };

        var p = _mapper.Map<Quote>(dto);
        p.Vendor.Should().Be("AlphaVantage");
        p.Id.Should().NotBeNull();
        p.Id.Symbol.Should().Be("IBM");
        p.Currency.Should().Be("USD");
        p.Price.Should().Be(123.45);
        p.Asof.Should().NotBeNull();
        p.Asof.ToDateTime().Kind.Should().Be(DateTimeKind.Utc);
    }
}
