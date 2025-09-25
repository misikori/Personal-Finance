using MarketGateway.Shared.DTOs;

namespace MarketGateway.Data.Services.Ohlcv;

public interface IOhlcvStorage
{
    Task<OhlcvSeriesDto?> TryReadAsync(MarketDataRequest req, CancellationToken ct);
    Task SaveAsync(OhlcvSeriesDto dto, CancellationToken ct);

    
    Task RecordParseFailureAsync(string vendor, DataType type, string identifier, string error,
        DateTime? occurredUtc = null, CancellationToken ct = default);
}