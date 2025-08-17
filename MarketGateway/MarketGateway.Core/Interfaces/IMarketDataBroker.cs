namespace MarketGateway.Interfaces;

public interface IMarketDataBroker
{
    Task<APIResult<MarketDataResultBase>> FetchAsync(
        MarketDataRequest request, CancellationToken ct);
}