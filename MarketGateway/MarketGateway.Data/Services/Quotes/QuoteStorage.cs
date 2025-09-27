using MarketGateway.Data.Entities;
using MarketGateway.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace MarketGateway.Data.Services.Quotes;

public sealed class QuoteStorage : IQuoteStorage
{
    private readonly MarketDbContext _db;
    public QuoteStorage(MarketDbContext db) => _db = db;

    public async Task<QuoteDto?> TryReadAsync(MarketDataRequest req, CancellationToken ct)
    {
        var symbol = ExtractSymbol(req);
        if (string.IsNullOrWhiteSpace(symbol)) return null;

        var cutoff = DateTime.UtcNow - TimeSpan.FromHours(24);
        var row = await _db.Quotes.AsNoTracking()
            .Where(q => q.Ticker == symbol && q.TimestampUtc >= cutoff)
            .OrderByDescending(q => q.TimestampUtc)
            .FirstOrDefaultAsync(ct);

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

    public async Task SaveAsync(QuoteDto dto, CancellationToken ct)
    {
        var entity = new QuoteEntity
        {
            Vendor       = string.IsNullOrWhiteSpace(dto.Vendor) ? "unknown" : dto.Vendor,
            Ticker       = ExtractTicker(dto.Id),
            TimestampUtc = dto.TimestampUtc?.UtcDateTime ?? DateTime.UtcNow,
            Price        = dto.Price,
            Open         = dto.Open,
            High         = dto.High,
            Low          = dto.Low,
            PrevClose    = dto.PrevClose,
            Volume       = dto.Volume,
            Currency     = dto.Currency
        };

        await _db.Quotes.AddAsync(entity, ct);
        await _db.SaveChangesAsync(ct);
    }

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

    private static string ExtractTicker(IdentifierDto? id)
        => string.IsNullOrWhiteSpace(id?.Symbol) ? "unknown" : id!.Symbol;
}
