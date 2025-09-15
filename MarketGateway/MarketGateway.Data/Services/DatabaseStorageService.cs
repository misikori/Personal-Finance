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
            case OhlcvSeriesDto series:
                await SaveOhlcvAsync(series, ct);
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
    
    private async Task SaveOhlcvAsync(OhlcvSeriesDto dto, CancellationToken ct)
    {

        var vendor = dto.Vendor;
        var symbol = dto.Id?.Symbol ?? "UNKNOWN";   
        var exchange = dto.Id?.Exchange;                   
        var currency = dto.Currency;                              
        var granularity = (int)dto.Granularity;                   
        var adjustment  = (int)dto.Adjustment;                   

        // Upsert/find parent Series row
        var series = await _db.OhlcvSeries.FirstOrDefaultAsync(x =>
            x.Vendor == vendor &&
            x.Symbol == symbol &&
            x.Exchange == exchange &&
            x.Currency == currency &&
            x.Granularity == granularity &&
            x.Adjustment == adjustment, ct);

        if (series is null)
        {
            series = new OhlcvSeriesEntity
            {
                Vendor = vendor,
                Symbol = symbol,
                Exchange = exchange,
                Currency = currency,
                Granularity = granularity,
                Adjustment = adjustment,
                Partial = dto.Partial,
                CreatedAtUtc = DateTime.UtcNow,
            };
            _db.OhlcvSeries.Add(series);
            await _db.SaveChangesAsync(ct); 
        }
        else
        {
            // Keep Partial updated (optional)
            series.Partial = dto.Partial;
            await _db.SaveChangesAsync(ct);
        }

        if (dto.Bars.Count == 0) return;

        // Collect timestamps to reduce N+1
        var tsList = dto.Bars.Select(b => b.TsUtc.UtcDateTime).ToList();
        var existing = await _db.OhlcvDaily
            .Where(x => x.SeriesId == series.Id && tsList.Contains(x.TimestampUtc))
            .ToDictionaryAsync(x => x.TimestampUtc, ct);

        var toInsert = new List<OhlcvDailyEntity>();

        foreach (var b in dto.Bars)
        {
            var ts = b.TsUtc.UtcDateTime;

            if (existing.TryGetValue(ts, out var row))
            {

                row.Open  = b.Open  ?? row.Open;
                row.High  = b.High  ?? row.High;
                row.Low   = b.Low   ?? row.Low;
                row.Close = b.Close ?? row.Close;

                if (b.Volume.HasValue)
                {
                    long? vol = null;
                    if (b.Volume.Value >= long.MinValue && b.Volume.Value <= long.MaxValue)
                        vol = (long)b.Volume.Value;
                    row.Volume = vol;
                }
            }
            else
            {
                long? vol = null;
                if (b.Volume.HasValue &&
                    b.Volume.Value >= long.MinValue &&
                    b.Volume.Value <= long.MaxValue)
                    vol = (long)b.Volume.Value;

                toInsert.Add(new OhlcvDailyEntity
                {
                    SeriesId = series.Id,
                    TimestampUtc = ts,
                    Open  = b.Open  ?? 0m,
                    High  = b.High  ?? 0m,
                    Low   = b.Low   ?? 0m,
                    Close = b.Close ?? 0m,
                    Volume = vol
                });
            }
        }

        if (toInsert.Count > 0)
            await _db.OhlcvDaily.AddRangeAsync(toInsert, ct);

        await _db.SaveChangesAsync(ct);
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
        if (r.Ids is { Count: > 0 } && !string.IsNullOrWhiteSpace(r.Ids[0].Symbol))
            return r.Ids[0].Symbol;

        // Optional: allow symbol from parameters (e.g., "symbol": "AAPL")
        if (r.Parameters is { Count: > 0 } &&
            r.Parameters.TryGetValue("symbol", out var s) &&
            s is string sym && !string.IsNullOrWhiteSpace(sym))
            return sym;

        return string.Empty;
    }
}