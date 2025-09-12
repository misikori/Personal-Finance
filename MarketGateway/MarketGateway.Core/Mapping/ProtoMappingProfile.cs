using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using MarketGateway.Contracts;
using MarketGateway.Shared.DTOs;

namespace MarketGateway.Mapping;

public sealed class ProtoMappingProfile : Profile
{
    public ProtoMappingProfile()
    {
        CreateMap<IdentifierDto, Identifier>()
            .ForMember(d => d.Symbol,   o => o.MapFrom(s => s.Symbol))
            .ForMember(d => d.Exchange, o => o.MapFrom(s => s.Exchange ?? string.Empty))
            .ForMember(d => d.AssetType,o => o.MapFrom(s => s.AssetType ?? string.Empty));

        CreateMap<QuoteDto, Quote>()
            .ForMember(d => d.Id,
                m => m.MapFrom(s => new Identifier { Symbol = s.Id.Symbol }))
            .ForMember(d => d.Price,
                m => m.MapFrom(s => (double?)s.Price ?? 0d))
            .ForMember(d => d.Open,
                m => m.MapFrom(s => (double?)s.Open ?? 0d))
            .ForMember(d => d.High,
                m => m.MapFrom(s => (double?)s.High ?? 0d))
            .ForMember(d => d.Low,
                m => m.MapFrom(s => (double?)s.Low ?? 0d))
            .ForMember(d => d.Volume,
                m => m.MapFrom(s => (double?)s.Volume ?? 0d))
            .ForMember(d => d.Currency,
                m => m.MapFrom(s => s.Currency ?? ""))
            .ForMember(d => d.Vendor,
                m => m.MapFrom(s => s.Vendor))
            .ForMember(d => d.Asof,
                m => m.MapFrom(s =>
                    s.TimestampUtc.HasValue
                        ? Timestamp.FromDateTime(DateTime.SpecifyKind(s.TimestampUtc.Value.DateTime, DateTimeKind.Utc))
                        : new Timestamp()))
            .ForMember(d => d.Meta, o => o.MapFrom(_ => new ResultMeta()));
    }
}