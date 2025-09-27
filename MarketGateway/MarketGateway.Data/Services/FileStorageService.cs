using System.Text.Json;
using System.Text.Json.Serialization;
using MarketGateway.Data.Interfaces;
using MarketGateway.Shared.DTOs;

namespace MarketGateway.Data.Services;

public class FileStorageService : IStorageService
{
    private readonly string _rootPath;
    private readonly JsonSerializerOptions _opts = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public FileStorageService(string rootPath = "DataStorage")
    {
        _rootPath = rootPath;
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<MarketDataResultBase?> TryReadAsync(MarketDataRequest request, CancellationToken ct = default)
    {
        return request.Type switch
        {
            DataType.Quote => await TryReadQuoteAsync(request, ct),
            _ => null
        };
    }

    private async Task<QuoteDto?> TryReadQuoteAsync(MarketDataRequest req, CancellationToken ct)
    {
        var symbol = ExtractSymbol(req);
        if (string.IsNullOrWhiteSpace(symbol)) return null;
        
        var freshness = TimeSpan.FromHours(24);
        var nowUtc    = DateTime.UtcNow;
        var cutoffUtc = nowUtc - freshness;

        var parsedRoot = Path.Combine(_rootPath, "parsed");

        if (!Directory.Exists(parsedRoot)) return null;

        foreach (var vendorDir in Directory.EnumerateDirectories(parsedRoot))
        {
            ct.ThrowIfCancellationRequested();

            var typeDir = Path.Combine(vendorDir, Sanitize(DataType.Quote.ToString()));
            if (!Directory.Exists(typeDir)) continue;
            
            foreach (var day in EnumerateDatesDescending(cutoffUtc.Date, nowUtc.Date))
            {
                ct.ThrowIfCancellationRequested();

                var dateDir = Path.Combine(typeDir, day.ToString("yyyy-MM-dd"));
                if (!Directory.Exists(dateDir)) continue;

                var prefix = Sanitize(symbol) + "_";
                var files = Directory.EnumerateFiles(dateDir, $"{prefix}*.json")
                                     .OrderByDescending(Path.GetFileName); 

                foreach (var file in files)
                {
                    ct.ThrowIfCancellationRequested();

                    QuoteDto? dto;
                    try
                    {
                        var json = await File.ReadAllTextAsync(file, ct).ConfigureAwait(false);
                        dto = JsonSerializer.Deserialize<QuoteDto>(json, _opts);
                    }
                    catch
                    {
                        // skip unreadable file
                        continue;
                    }

                    if (dto is null) continue;
                    if (dto.Type != DataType.Quote) continue;
                    if (string.IsNullOrWhiteSpace(dto.Id?.Symbol)) continue;
                    if (!dto.TimestampUtc.HasValue) continue;

                    var tsUtc = dto.TimestampUtc.Value.UtcDateTime;
                    if (tsUtc < cutoffUtc) continue; 
                    
                    return dto with
                    {
                        Vendor = string.IsNullOrWhiteSpace(dto.Vendor) ? "unknown" : dto.Vendor,
                        Id     = new IdentifierDto(dto.Id.Symbol)
                    };
                }
            }
        }

        return null;
    }
    
    public async Task SaveParsedResultAsync(MarketDataResultBase result, CancellationToken ct = default)
    {
        var vendor = Sanitize(result.Vendor);
        var type   = Sanitize(result.Type.ToString());
        var ident  = Sanitize(ResolveTicker(result.Id));
        var ts     = ResolveUtc(ExtractTimestampUtc(result));

        var dateFolder = Path.Combine(_rootPath, "parsed", vendor, type, ts.ToString("yyyy-MM-dd"));
        Directory.CreateDirectory(dateFolder);

        var finalPath = Path.Combine(dateFolder, $"{ident}_{ts:HHmmssfff}.json");
        var tmpPath   = finalPath + ".tmp";

        var json = JsonSerializer.Serialize(result, result.GetType(), _opts);
        
        await File.WriteAllTextAsync(tmpPath, json, ct).ConfigureAwait(false);
        File.Move(tmpPath, finalPath, overwrite: false);
    }

    public async Task RecordParseFailureAsync(string vendor, DataType type, string identifier, string error,
                                              DateTime? occurredUtc = null, CancellationToken ct = default)
    {
        var v  = Sanitize(vendor);
        var t  = Sanitize(type.ToString());
        var ts = occurredUtc ?? DateTime.UtcNow;

        var dateFolder = Path.Combine(_rootPath, "failures", v, t, ts.ToString("yyyy-MM-dd"));
        Directory.CreateDirectory(dateFolder);

        var file = Path.Combine(dateFolder, "failures.ndjson");
        var record = new
        {
            vendor,
            type = type.ToString(),
            identifier,
            occurredAtUtc = ts,
            error
        };
        var line = JsonSerializer.Serialize(record, _opts);
        
        await using var fs = new FileStream(file,
            File.Exists(file) ? FileMode.Append : FileMode.CreateNew,
            FileAccess.Write, FileShare.Read);
        await using var sw = new StreamWriter(fs);
        await sw.WriteLineAsync(line).ConfigureAwait(false);
    }
    
    private static IEnumerable<DateTime> EnumerateDatesDescending(DateTime fromInclusiveUtc, DateTime toInclusiveUtc)
    {
        for (var d = toInclusiveUtc.Date; d >= fromInclusiveUtc.Date; d = d.AddDays(-1))
            yield return d;
    }

    private static string Sanitize(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "unknown";
        var bad = Path.GetInvalidFileNameChars();
        return new string(s.Select(ch => bad.Contains(ch) ? '_' : ch).ToArray());
    }

    private static DateTimeOffset? ExtractTimestampUtc(MarketDataResultBase result)
    {
        var pi = result.GetType().GetProperty("TimestampUtc");
        if (pi?.GetValue(result) is DateTimeOffset dtoff) return dtoff;
        if (pi?.GetValue(result) is DateTime dt)
            return new DateTimeOffset(dt.Kind == DateTimeKind.Utc ? dt : dt.ToUniversalTime());
        return null;
    }

    private static DateTime ResolveUtc(DateTimeOffset? dtoTs)
        => dtoTs?.UtcDateTime ?? DateTime.UtcNow;

    private static string ResolveTicker(IdentifierDto? id)
        => string.IsNullOrWhiteSpace(id?.Symbol) ? "unknown" : id.Symbol;
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
}
