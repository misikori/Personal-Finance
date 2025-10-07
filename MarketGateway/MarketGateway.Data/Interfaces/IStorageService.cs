using MarketGateway.Shared.DTOs;

namespace MarketGateway.Data.Interfaces;

public interface IStorageService
{
    /// <summary>Persist a successfully parsed DTO into its normalized table.</summary>
    Task SaveParsedResultAsync(MarketDataResultBase result, CancellationToken ct = default);

    /// <summary>Record that parsing/mapping failed (no raw JSON persisted).</summary>
    Task RecordParseFailureAsync(string vendor, DataType type, string identifier, string error,
        DateTime? occurredUtc = null, CancellationToken ct = default);
    
    Task<MarketDataResultBase?> TryReadAsync(MarketDataRequest request, CancellationToken ct = default);

    
}