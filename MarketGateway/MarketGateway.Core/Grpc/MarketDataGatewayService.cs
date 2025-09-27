using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MarketGateway.Contracts;
using MarketGateway.Interfaces;
using MarketGateway.Shared.DTOs;
using Serilog;
using static MarketGateway.Contracts.Utils.ProtoJson;
using DataType = MarketGateway.Shared.DTOs.DataType;

namespace MarketGateway.Grpc;

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
        _log.LogInformation("Fetching market data");
        _log.LogInformation($"Fetching market data for {req}");
        var requestId = Guid.NewGuid().ToString("N");
        using var scope = _log.BeginScope(new Dictionary<string, object?>
        {
            ["requestId"] = requestId
        });
        
        var ids = req.Ids.Select(i =>
                new IdentifierDto(i.Symbol?.Trim() ?? string.Empty,
                    string.IsNullOrWhiteSpace(i.Exchange) ? null : i.Exchange,
                    string.IsNullOrWhiteSpace(i.AssetType) ? null : i.AssetType))
            .ToList();

        var range = new TimeRangeDto(
            req.Range?.Start?.ToDateTime().ToUniversalTime(),
            req.Range?.End?.ToDateTime().ToUniversalTime());
        
        var mappedType = MapDataType(req.DataType);
        _log.LogInformation("Fetch received: protoType={ProtoType} mappedType={MappedType} ids={Ids} " +
                            "range=({Start}..{End}) preferredVendors={PreferredVendors} hasParams={HasParams}",
            req.DataType, mappedType, string.Join(",", ids.Select(x => x.Symbol)),
            range.Start, range.End,
            req.Options?.PreferredVendors?.Count ?? 0,
            req.Parameters?.Fields?.Count > 0);
        
        if (mappedType == 0)
        {
            _log.LogWarning("Rejecting request: data_type unspecified.");
            return new FetchReply
            {
                Ok = false,
                Error = "data_type must be set (e.g., QUOTE or STOCK_PRICE)",
                RequestId = requestId
            };
        }
        
        var dto = new MarketDataRequest
        {
            Type = mappedType,
            Ids  = ids,
            Range = range,
            Parameters = ToDictionary(req.Parameters),
            PreferredVendors = req.Options?.PreferredVendors?.ToList() ?? new()
        };
        Log.Information("This is MarketDataRequest: {Request}", dto);
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
            case OhlcvSeriesDto q:
                reply.OhlcvSeries = _mapper.Map<OhlcvSeries>(q);
                break;
            default:
                return new FetchReply { Ok = false, Error = "Unsupported result type", RequestId = requestId };
        }
        return reply;
    }
    
    private static DataType MapDataType(MarketGateway.Contracts.DataType t)
        => t switch
        {
            MarketGateway.Contracts.DataType.Quote       => DataType.Quote,
            MarketGateway.Contracts.DataType.Ohlcv => DataType.OHLCV,
            _ => 0
        };


}