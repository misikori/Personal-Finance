using MarketGateway.Providers.Interfaces;
using MarketGateway.Shared.DTOs;

namespace MarketGateway.Interfaces;

public interface IMarketDataProviderResolver
{
    bool TryGet(string vendor, out IMarketDataProvider provider);
    IReadOnlyDictionary<string, IMarketDataProvider> GetAll();

    /// <summary>Find providers that can handle this request (type, params).</summary>
    IEnumerable<IMarketDataProvider> FindByRequest(MarketDataRequest request);
}
