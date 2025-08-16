    using System.ComponentModel.DataAnnotations;
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
        
        /// <summary>
        /// Checking if we can send request to the required Vendor
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<(bool Allowed, string? Reason, DateTimeOffset? RetryAfter)> CanFetchDataAsync(MarketDataRequest request, CancellationToken cancellationToken);
        Task<APIResult<MarketDataResultBase>> FetchAsync(MarketDataRequest request, CancellationToken cancellationToken);
    }