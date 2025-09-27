using MarketGateway.Shared.API;
using MarketGateway.Shared.DTOs;

namespace MarketGateway.Interfaces;

//TODO: Check if we need pagination if window data is too long or something like that 

public interface IMarketDataBroker
{
    /// <summary>Fetch a single logical request (may contain multiple Ids).</summary>
    Task<ApiResult<MarketDataResultBase>> FetchAsync(MarketDataRequest request, CancellationToken ct = default);

    /// <summary>Stream results for large/many-Id requests as they arrive.</summary>
    IAsyncEnumerable<ApiResult<MarketDataResultBase>> FetchStreamAsync(
        MarketDataRequest request,
        CancellationToken ct = default);
}