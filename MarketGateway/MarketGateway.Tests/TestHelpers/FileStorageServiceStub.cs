using MarketGateway.Data.Interfaces;
using MarketGateway.Shared.DTOs;


namespace MarketGateway.Tests.TestHelpers;

public sealed class InMemoryStorageService : IStorageService
{
    private readonly List<MarketDataResultBase> Parsed = new();
    private readonly List<(string Vendor, DataType Type, string Identifier, string Error, DateTime OccurredUtc)> Failures = new();
    
    public void SeedParsedQuote(
        string vendor, string symbol, decimal price,
        DateTimeOffset? timestampUtc = null,
        decimal? open = null, decimal? high = null, decimal? low = null,
        decimal? prevClose = null, long? volume = null, string? currency = null)
    {
        var dto = new QuoteDto
        {
            Vendor       = vendor,
            Type         = DataType.Quote,
            Id           = new IdentifierDto(symbol),
            TimestampUtc = timestampUtc ?? DateTimeOffset.UtcNow,
            Price        = price,
            Open         = open,
            High         = high,
            Low          = low,
            PrevClose    = prevClose,
            Volume       = volume,
            Currency     = currency
        };
        Parsed.Add(dto);
    }
    public Task SaveParsedResultAsync(MarketDataResultBase result, CancellationToken ct = default)
    {
        // Upsert by (Type, Id) with latest timestamp (simple policy for tests)
        if (result is QuoteDto q && q.Id is not null)
        {
            Parsed.RemoveAll(x =>
                x is QuoteDto qq &&
                qq.Id?.Symbol?.Equals(q.Id.Symbol, StringComparison.OrdinalIgnoreCase) == true);

            if (q.TimestampUtc == default)
                q.TimestampUtc = DateTimeOffset.UtcNow;

            Parsed.Add(q);
        }
        else
        {
            // For other DTO types later, just append
            Parsed.Add(result);
        }

        return Task.CompletedTask;
    }
    public Task RecordParseFailureAsync(
        string vendor, DataType type, string identifier, string error,
        DateTime? occurredUtc = null, CancellationToken ct = default)
    {
        Failures.Add((vendor, type, identifier, error, occurredUtc ?? DateTime.UtcNow));
        return Task.CompletedTask;
    }

    public Task<MarketDataResultBase?> TryReadAsync(MarketDataRequest request, CancellationToken ct = default)
    {
        if (request.Type == DataType.Quote)
        {
            var symbol = request?.Ids?.FirstOrDefault()?.Symbol ?? string.Empty;
            if (string.IsNullOrWhiteSpace(symbol))
                return Task.FromResult<MarketDataResultBase?>(null);

            // Mirror DB service’s 24h freshness contract
            var cutoff = DateTimeOffset.UtcNow - TimeSpan.FromHours(24);

            var hit = Parsed
                .OfType<QuoteDto>()
                .Where(q =>
                    q.Id?.Symbol?.Equals(symbol, StringComparison.OrdinalIgnoreCase) == true &&
                    (q.TimestampUtc == default || q.TimestampUtc >= cutoff))
                .OrderByDescending(q => q.TimestampUtc)
                .FirstOrDefault();

            return Task.FromResult<MarketDataResultBase?>(hit);
        }

        // Extend with other types as added
        return Task.FromResult<MarketDataResultBase?>(null);
    }
}
