using MarketGateway.Data.Interfaces;
using MarketGateway.Shared.DTOs;


namespace MarketGateway.Tests.TestHelpers;

public sealed class InMemoryStorageService : IStorageService
{
    public readonly List<(string Vendor, string Key, string Json, DateTime Utc)> RawCalls = new();
    public readonly List<MarketDataResultBase> Parsed = new();

    // Optional: allow pre-seeding raw storage for tests (e.g., storage-first scenarios)
    public void SeedRaw(string vendor, string identifier, string json, DateTime? utc = null)
        => RawCalls.Add((vendor, identifier, json, utc ?? DateTime.UtcNow));

    public Task SaveApiResponseAsync(string vendor, string identifier, string json, CancellationToken ct = default)
    {
        RawCalls.Add((vendor, identifier, json, DateTime.UtcNow));
        return Task.CompletedTask;
    }

    public Task<string?> TryReadLatestAsync(string vendor, string identifier, DateTime? dateUtc = null, CancellationToken ct = default)
    {
        var when = (dateUtc ?? DateTime.UtcNow).Date;
        // Find latest for the same vendor/identifier on the given date
        var latest = RawCalls
            .Where(x => string.Equals(x.Vendor, vendor, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(x.Key, identifier, StringComparison.OrdinalIgnoreCase)
                        && x.Utc.Date == when)
            .OrderByDescending(x => x.Utc)
            .Select(x => x.Json)
            .FirstOrDefault();

        return Task.FromResult(latest);
    }

    public Task<IEnumerable<string>> GetSavedFilesAsync(string vendor, DateTime dateUtc, CancellationToken ct = default)
    {
        var files = RawCalls
            .Where(x => string.Equals(x.Vendor, vendor, StringComparison.OrdinalIgnoreCase)
                        && x.Utc.Date == dateUtc.Date)
            .Select(x => $"{x.Key}@{x.Utc:HHmmssfff}") // just a synthetic name
            .ToArray()
            .AsEnumerable();
        return Task.FromResult(files);
    }

    public Task SaveParsedResultAsync(MarketDataResultBase result)
    {
        Parsed.Add(result);
        return Task.CompletedTask;
    }
}
