using MarketGateway.Shared.DTOs;

namespace MarketGateway.Data.Interfaces;

public interface IStorageService
{
    Task SaveApiResponseAsync(string vendor, string identifier, string json, CancellationToken ct = default);
    Task<string?> TryReadLatestAsync(string vendor, string identifier, DateTime? dateUtc = null, CancellationToken ct = default);
    Task<IEnumerable<string>> GetSavedFilesAsync(string vendor, DateTime dateUtc, CancellationToken ct = default);
    Task SaveParsedResultAsync(MarketDataResultBase result); 

}