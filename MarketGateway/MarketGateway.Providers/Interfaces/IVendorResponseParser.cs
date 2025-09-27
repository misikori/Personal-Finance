using MarketGateway.Providers.Configuration;
using MarketGateway.Shared.DTOs;

namespace MarketGateway.Providers.Interfaces;

public interface IVendorResponseParser
{
 
    /// <summary>
    /// Parse a vendor response (JSON/CSV/XML) to a strongly-typed result. Never throws;
    /// returns Failure with an error message when parsing fails.
    /// </summary>
    MarketDataResultBase Parse(VendorConfig config, MarketDataRequest request, string json);
}