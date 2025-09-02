using MarketGateway.Shared.API;
    using MarketGateway.Shared.Configuration;
    using MarketGateway.Shared.DTOs;

    namespace MarketGateway.Interfaces;

    
    /// <summary>
    /// Generic Interface for a market data provided by YAML configs.
    /// </summary>
    public interface IMarketDataProvider
    {
        string VendorName { get; }
        VendorConfig Config { get; }
        
        /// <summary>Check whether a live fetch is currently allowed (rate limits, vendor health, etc.).</summary>
        Task<FetchGate> CanFetchDataAsync(MarketDataRequest request, CancellationToken cancellationToken = default);

        /// <summary>Perform a live fetch. Caller should honor the gate first.</summary>
        Task<APIResult<MarketDataResultBase>> FetchAsync(MarketDataRequest request, CancellationToken cancellationToken = default);
    }