using MarketGateway.Data.Entities;
using MarketGateway.Data.Interfaces;
using MarketGateway.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MarketGateway.Data.Services.Ohlcv;

public sealed class OhlcvStorage : IOhlcvStorage
{
    private readonly MarketDbContext _db;
    private readonly IAppDate _appDate;
    public OhlcvStorage(MarketDbContext db, IAppDate appDate)
    {
        _db = db;
        _appDate = appDate;
    }

    public async Task<OhlcvSeriesDto?> TryReadAsync(MarketDataRequest req, CancellationToken ct)
    {
        var symbol = ExtractSymbol(req);
        if (string.IsNullOrWhiteSpace(symbol))
        {
            Log.Warning("OHLCV read aborted: no symbol in request.");
            return null;
        }

        var appDate = _appDate.Today;
        var (granProvided, granularity) = TryExtractGranularity(req);
        var (adjProvided, adjustment)   = TryExtractAdjustment(req);
        var preferred = req.PreferredVendors ?? new List<string>();

        Log.Information("OHLCV read start symbol={Symbol} date={Date} gran={Gran}(provided={GranProvided}) adj={Adj}(provided={AdjProvided}) prefVendors=[{Pref}]",
            symbol, appDate, granularity, granProvided, adjustment, adjProvided, string.Join(",", preferred));
        
        var q = _db.OhlcvSeries.AsNoTracking()
            .Where(s => s.Symbol == symbol && DateOnly.FromDateTime(s.CreatedAtUtc) == appDate);

        var initialCount = await q.CountAsync(ct);
        Log.Information("OHLCV candidate series on date={Date}: {Count}", appDate, initialCount);

        if (preferred.Count > 0)
        {
            q = q.Where(s => preferred.Contains(s.Vendor));
            var pvCount = await q.CountAsync(ct);
            Log.Information("After preferred vendor filter: {Count}", pvCount);
        }

        if (granProvided)
        {
            q = q.Where(s => s.Granularity == (int)granularity);
            var gCount = await q.CountAsync(ct);
            Log.Information("After granularity={Gran} filter: {Count}", granularity, gCount);
        }

        if (adjProvided)
        {
            q = q.Where(s => s.Adjustment == (int)adjustment);
            var aCount = await q.CountAsync(ct);
            Log.Information("After adjustment={Adj} filter: {Count}", adjustment, aCount);
        }

        var series = await q.OrderByDescending(s => s.CreatedAtUtc).FirstOrDefaultAsync(ct);

        if (series is null)
        {
            Log.Warning("No series matched strict filters for {Symbol} on {Date}. Falling back to relaxed search.", symbol, appDate);
            
            var relaxed = _db.OhlcvSeries.AsNoTracking()
                .Where(s => s.Symbol == symbol && DateOnly.FromDateTime(s.CreatedAtUtc) == appDate);

            if (preferred.Count > 0)
                relaxed = relaxed.Where(s => preferred.Contains(s.Vendor));

            var relaxedSeries = await relaxed.OrderByDescending(s => s.CreatedAtUtc).FirstOrDefaultAsync(ct);
            if (relaxedSeries is null)
            {
                Log.Warning("No series found for symbol={Symbol} on date={Date} at all.", symbol, appDate);
                
                var anyDate = await _db.OhlcvSeries.AsNoTracking()
                    .Where(s => s.Symbol == symbol)
                    .OrderByDescending(s => s.CreatedAtUtc)
                    .FirstOrDefaultAsync(ct);

                if (anyDate is null)
                {
                    Log.Warning("No series found for symbol={Symbol} on ANY date.", symbol);
                    return null;
                }

                Log.Warning("Using fallback ANY-DATE series createdAt={Created} (dev mode).", anyDate.CreatedAtUtc);
                series = anyDate;
            }
            else
            {
                Log.Information("Relaxed match found for date={Date} createdAt={Created}.", appDate, relaxedSeries.CreatedAtUtc);
                series = relaxedSeries;
            }
        }
        
        var bars = await _db.OhlcvDaily.AsNoTracking()
            .Where(b => b.SeriesId == series.Id)
            .OrderBy(b => b.TimestampUtc)
            .Select(b => new OhlcvBarDto
            {
                TsUtc  = new DateTimeOffset(DateTime.SpecifyKind(b.TimestampUtc, DateTimeKind.Utc)),
                Open   = b.Open,
                High   = b.High,
                Low    = b.Low,
                Close  = b.Close,
                Volume = b.Volume
            })
            .ToListAsync(ct);

        Log.Information("Returning OHLCV series vendor={Vendor} gran={Gran} adj={Adj} bars={BarsCount}",
            series.Vendor, (BarGranularity)series.Granularity, (PriceAdjustment)series.Adjustment, bars.Count);

        return new OhlcvSeriesDto
        {
            Vendor      = series.Vendor,
            Type        = DataType.OHLCV,
            Id          = new IdentifierDto(symbol) { Exchange = series.Exchange },
            Currency    = series.Currency,
            Granularity = (BarGranularity)series.Granularity,
            Adjustment  = (PriceAdjustment)series.Adjustment,
            Partial     = series.Partial,
            Bars        = bars
        };
    }


    public async Task SaveAsync(OhlcvSeriesDto dto, CancellationToken ct)
    {
        var vendor      = dto.Vendor;
        var symbol      = dto.Id?.Symbol ?? "UNKNOWN";
        var exchange    = dto.Id?.Exchange;
        var currency    = dto.Currency;
        var granularity = (int)dto.Granularity;
        var adjustment  = (int)dto.Adjustment;

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
                Vendor       = vendor,
                Symbol       = symbol,
                Exchange     = exchange,
                Currency     = currency,
                Granularity  = granularity,
                Adjustment   = adjustment,
                Partial      = dto.Partial,
                CreatedAtUtc = DateTime.UtcNow,
            };
            _db.OhlcvSeries.Add(series);
            await _db.SaveChangesAsync(ct);
        }
        else
        {
            series.Partial = dto.Partial;
            await _db.SaveChangesAsync(ct);
        }

        if (dto.Bars.Count == 0) return;

        var tsList   = dto.Bars.Select(b => b.TsUtc.UtcDateTime).ToList();
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
                    SeriesId     = series.Id,
                    TimestampUtc = ts,
                    Open         = b.Open  ?? 0m,
                    High         = b.High  ?? 0m,
                    Low          = b.Low   ?? 0m,
                    Close        = b.Close ?? 0m,
                    Volume       = vol
                });
            }
        }

        if (toInsert.Count > 0)
            await _db.OhlcvDaily.AddRangeAsync(toInsert, ct);

        await _db.SaveChangesAsync(ct);
    }

    public async Task RecordParseFailureAsync(string vendor, DataType type, string identifier, string error,
        DateTime? occurredUtc = null, CancellationToken ct = default)
    {
        _db.ParseFailures.Add(new ParseFailure
        {
            Vendor         = vendor,
            Type           = type,
            PrimaryIdentifier = identifier,
            OccurredAtUtc  = occurredUtc ?? DateTime.UtcNow,
            Error          = error
        });
        await _db.SaveChangesAsync(ct);
    }

    // --- small helpers ---
    private static string ExtractSymbol(MarketDataRequest r)
    {
        if (r.Ids is { Count: > 0 } && !string.IsNullOrWhiteSpace(r.Ids[0].Symbol))
            return r.Ids[0].Symbol;
        if (r.Parameters is { Count: > 0 } &&
            r.Parameters.TryGetValue("symbol", out var s) &&
            s is string sym && !string.IsNullOrWhiteSpace(sym))
            return sym;
        return string.Empty;
    }

    private static BarGranularity ExtractGranularity(MarketDataRequest req, BarGranularity fallback)
        => (req.Parameters is { Count: > 0 } &&
            req.Parameters.TryGetValue("granularity", out var g) &&
            g is string gs &&
            Enum.TryParse<BarGranularity>(gs, true, out var parsed))
            ? parsed : fallback;

    private static PriceAdjustment ExtractAdjustment(MarketDataRequest req, PriceAdjustment fallback)
        => (req.Parameters is { Count: > 0 } &&
            req.Parameters.TryGetValue("adjustment", out var a) &&
            a is string asx &&
            Enum.TryParse<PriceAdjustment>(asx, true, out var parsed))
            ? parsed : fallback;
    
    private static (bool provided, BarGranularity value) TryExtractGranularity(MarketDataRequest req)
    {
        if (req.Parameters is { Count: > 0 } &&
            req.Parameters.TryGetValue("granularity", out var g) &&
            g is string gs &&
            Enum.TryParse<BarGranularity>(gs, true, out var parsed))
            return (true, parsed);

        return (false, default); 
    }

    private static (bool provided, PriceAdjustment value) TryExtractAdjustment(MarketDataRequest req)
    {
        if (req.Parameters is { Count: > 0 } &&
            req.Parameters.TryGetValue("adjustment", out var a) &&
            a is string asx &&
            Enum.TryParse<PriceAdjustment>(asx, true, out var parsed))
            return (true, parsed);

        return (false, default); 
    }
}
