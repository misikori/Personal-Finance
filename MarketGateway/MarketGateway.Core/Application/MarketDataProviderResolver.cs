using MarketGateway.Interfaces;
using MarketGateway.Providers.Interfaces;
using MarketGateway.Shared.DTOs;

namespace MarketGateway.Application;



public sealed class MarketDataProviderResolver : IMarketDataProviderResolver
{
    private readonly IReadOnlyDictionary<string, IMarketDataProvider> _map;
    private readonly ILogger<MarketDataProviderResolver> _log;

    public MarketDataProviderResolver(IEnumerable<IMarketDataProvider> providers, ILogger<MarketDataProviderResolver> log)
    {
        var list = providers.ToList();
        _map = list.ToDictionary(p => p.VendorName, StringComparer.OrdinalIgnoreCase);
        _log = log;
        _log.LogInformation("Resolver initialized with {Count} provider(s): {Vendors}",
            _map.Count, string.Join(", ", _map.Keys));
    }
    
    public bool TryGet(string vendor, out IMarketDataProvider provider) =>
        _map.TryGetValue(vendor, out provider!);

    public IReadOnlyDictionary<string, IMarketDataProvider> GetAll() => _map;

    public IEnumerable<IMarketDataProvider> FindByRequest(MarketDataRequest request) =>
        _map.Values.Where(p => p.Config.Supports(request.Type));
}