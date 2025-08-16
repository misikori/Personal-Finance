namespace MarketGateway.Interfaces;

public interface IMarketDataProviderResolver
{
    IMarketDataProvider Get(string vendor);
}

public sealed class MarketDataProviderResolver : IMarketDataProviderResolver
{
    private readonly IReadOnlyDictionary<string, IMarketDataProvider> _map;
    public MarketDataProviderResolver(IReadOnlyDictionary<string, IMarketDataProvider> map) => _map = map;
    public IMarketDataProvider Get(string vendor) => _map[vendor];
}