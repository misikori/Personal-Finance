using MarketGateway.Data.Entities;
using MarketGateway.Data.Interfaces;
using MarketGateway.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace MarketGateway.Data.Services;

public sealed class DatabaseStorageService : IStorageService
{
    private readonly MarketDbContext _db;
    public DatabaseStorageService(MarketDbContext db) => _db = db;
    
    public async Task<MarketDataResultBase?> TryReadAsync(MarketDataRequest request, CancellationToken ct = default)
    {
        switch (request.Type)
        {
            case DataType.Quote:
                return await TryReadQuoteAsync(request, ct);
            
            default:
                return null;
        }
    }

    public async Task SaveParsedResultAsync(MarketDataResultBase result, CancellationToken ct = default)
    {
        switch (result)
        {
            case QuoteDto q:
                await SaveQuoteAsync(q, ct);
                break;
            //TODO: Add all cases

            default:
                throw new NotSupportedException($"No storage mapping for {result.GetType().Name} ({result.Type})");
        }

        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task RecordParseFailureAsync(string vendor, DataType type, string identifier, string error,
        DateTime? occurredUtc = null, CancellationToken ct = default)
    {
        _db.ParseFailures.Add(new ParseFailure
        {
            Vendor = vendor,
            Type = type,
            PrimaryIdentifier = identifier,
            OccurredAtUtc = occurredUtc ?? DateTime.UtcNow,
            Error = error
        });
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    private static DateTime ResolveUtc(DateTimeOffset? dtoTs)
        => dtoTs?.UtcDateTime ?? DateTime.UtcNow;

    private static string ResolveTicker(IdentifierDto? id)
        => string.IsNullOrWhiteSpace(id?.Symbol) ? "unknown" : id.Symbol;

    private async Task SaveQuoteAsync(QuoteDto dto, CancellationToken ct)
    {
        var entity = new QuoteEntity
        {
            Vendor = string.IsNullOrWhiteSpace(dto.Vendor) ? "unknown" : dto.Vendor,
            Ticker = ResolveTicker(dto.Id),
            TimestampUtc = ResolveUtc(dto.TimestampUtc),
            Price = dto.Price,
            Open = dto.Open,
            High = dto.High,
            Low = dto.Low,
            PrevClose = dto.PrevClose,
            Volume = dto.Volume,
            Currency = dto.Currency
        };

        await _db.Quotes.AddAsync(entity, ct);
    }
    
    
    private async Task<QuoteDto?> TryReadQuoteAsync(MarketDataRequest req, CancellationToken ct)
    {
        var symbol = ExtractSymbol(req);
        if (string.IsNullOrWhiteSpace(symbol)) return null;
        
        var freshness = TimeSpan.FromHours(24);
        var cutoff = DateTime.UtcNow - freshness;

        var row = await _db.Quotes.AsNoTracking()
            .Where(q => q.Ticker == symbol && q.TimestampUtc >= cutoff)
            .OrderByDescending(q => q.TimestampUtc)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

        if (row is null) return null;

        return new QuoteDto
        {
            Vendor       = row.Vendor,
            Type         = DataType.Quote,
            Id           = new IdentifierDto(symbol),
            TimestampUtc = new DateTimeOffset(DateTime.SpecifyKind(row.TimestampUtc, DateTimeKind.Utc)),
            Price        = row.Price,
            Open         = row.Open,
            High         = row.High,
            Low          = row.Low,
            PrevClose    = row.PrevClose,
            Volume       = row.Volume,
            Currency     = row.Currency
        };
    }
    
    private static string ExtractSymbol(MarketDataRequest r)
    {
        if (r?.Ids is { Count: > 0 } && !string.IsNullOrWhiteSpace(r.Ids[0].Symbol))
            return r.Ids[0].Symbol;

        // Optional: allow symbol from parameters (e.g., "symbol": "AAPL")
        if (r?.Parameters is { Count: > 0 } &&
            r.Parameters.TryGetValue("symbol", out var s) &&
            s is string sym && !string.IsNullOrWhiteSpace(sym))
            return sym;

        return string.Empty;
    }
}