using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MarketGateway.Contracts;
using MarketGateway.Interfaces;
using MarketGateway.Shared.DTOs;
using static MarketGateway.Shared.ProtoJson;
using DataType = MarketGateway.Shared.DTOs.DataType;

namespace MarketGateway.Services;

public class MarketDataGatewayService : MarketDataGateway.MarketDataGatewayBase
{
    private readonly IMarketDataBroker _broker;
    private readonly IMapper _mapper;
    private readonly ILogger<MarketDataGatewayService> _log;
    public MarketDataGatewayService(
        IMarketDataBroker broker,
        ILogger<MarketDataGatewayService> log,
        IMapper mapper)
    {
        _broker = broker;
        _log    = log;
        _mapper = mapper;
    }

    public override async Task<FetchReply> Fetch(FetchRequest req, ServerCallContext ctx)
    {
        var requestId = Guid.NewGuid().ToString("N");

        var ids = req.Ids.Select(i =>
                new IdentifierDto(i.Symbol?.Trim() ?? string.Empty,
                    string.IsNullOrWhiteSpace(i.Exchange) ? null : i.Exchange,
                    string.IsNullOrWhiteSpace(i.AssetType) ? null : i.AssetType))
            .ToList();

        var range = new TimeRangeDto(
            req.Range?.Start?.ToDateTime().ToUniversalTime(),
            req.Range?.End?.ToDateTime().ToUniversalTime());
        
        var dto = new MarketDataRequest
        {
            Type = (DataType)req.DataType,
            Ids  = ids,
            Range = range,
            Parameters = ToDictionary(req.Parameters),
            PreferredVendors = req.Options?.PreferredVendors?.ToList() ?? new()
        };

        var result = await _broker.FetchAsync(dto, ctx.CancellationToken);

        if (!result.Success || result.Data is null)
            return new FetchReply
            {
                Ok = false, 
                Error = result.Error ?? "Unknown error",
                RetryAfter = result.RetryAfter?.ToTimestamp(), 
                RequestId = requestId
            };
        
        var reply = new FetchReply { Ok = true, RequestId = requestId };
        switch (result.Data)
        {
            case QuoteDto q:
                reply.Quote = _mapper.Map<Quote>(q);
                break;
            default:
                return new FetchReply { Ok = false, Error = "Unsupported result type", RequestId = requestId };
        }
        return reply;
    }


}