using MarketGateway.Interfaces;
using MarketGateway.Shared.DTOs;

namespace MarketGateway.Services;



public sealed class MarketDataProviderResolver : IMarketDataProviderResolver
{
    private readonly IReadOnlyDictionary<string, IMarketDataProvider> _map;

    public MarketDataProviderResolver(IReadOnlyDictionary<string, IMarketDataProvider> map)
        => _map = map;

    public bool TryGet(string vendor, out IMarketDataProvider provider) =>
        _map.TryGetValue(vendor, out provider!);

    public IReadOnlyDictionary<string, IMarketDataProvider> GetAll() => _map;

    public IEnumerable<IMarketDataProvider> FindByRequest(MarketDataRequest request) =>
        _map.Values.Where(p => p.Config.Supports(request.Type));
}