using System.Text.Json;
using System.Text.Json.Serialization;
using MarketGateway.Data.Interfaces;
using MarketGateway.Shared.DTOs;

namespace MarketGateway.Data;

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

    public async Task SaveParsedResultAsync(MarketDataResultBase result, CancellationToken ct = default)
    {
        var vendor = Sanitize(result.Vendor ?? "unknown");
        var type   = Sanitize(result.Type.ToString());
        var ident  = Sanitize(ResolveTicker(result.Id));
        var ts     = ResolveUtc(ExtractTimestampUtc(result));

        var dateFolder = Path.Combine(_rootPath, "parsed", vendor, type, ts.ToString("yyyy-MM-dd"));
        Directory.CreateDirectory(dateFolder);

        var file = Path.Combine(dateFolder, $"{ident}_{ts:HHmmssfff}.json");

        var json = JsonSerializer.Serialize(result, result.GetType(), _opts);
        await File.WriteAllTextAsync(file, json, ct);
    }

    public async Task RecordParseFailureAsync(string vendor, DataType type, string identifier, string error,
                                              DateTime? occurredUtc = null, CancellationToken ct = default)
    {
        var v   = Sanitize(vendor);
        var t   = Sanitize(type.ToString());
        var ts  = occurredUtc ?? DateTime.UtcNow;
        
        var dateFolder = Path.Combine(_rootPath, "failures", v, t, ts.ToString("yyyy-MM-dd"));
        Directory.CreateDirectory(dateFolder);
        var file = Path.Combine(dateFolder, "failures.ndjson");

        var record = new
        {
            vendor,
            type   = type.ToString(),
            identifier,
            occurredAtUtc = ts,
            error
        };
        var line = JsonSerializer.Serialize(record, _opts);

        await using var fs = new FileStream(file, File.Exists(file) ? FileMode.Append : FileMode.CreateNew, FileAccess.Write, FileShare.Read);
        await using var sw = new StreamWriter(fs);
        await sw.WriteLineAsync(line.AsMemory(), ct).ConfigureAwait(false);
    }

    private static string Sanitize(string s)
    {
        var bad = Path.GetInvalidFileNameChars();
        return new string(s.Select(ch => bad.Contains(ch) ? '_' : ch).ToArray());
    }
    private static DateTimeOffset? ExtractTimestampUtc(MarketDataResultBase result)
    {
        var pi = result.GetType().GetProperty("TimestampUtc");
        if (pi?.GetValue(result) is DateTimeOffset dtoff) return dtoff;
        if (pi?.GetValue(result) is DateTime dt) return new DateTimeOffset(
            dt.Kind == DateTimeKind.Utc ? dt : dt.ToUniversalTime());
        return null;
    }
    
    private static DateTime ResolveUtc(DateTimeOffset? dtoTs)
        => dtoTs?.UtcDateTime ?? DateTime.UtcNow;

    private static string ResolveTicker(IdentifierDto? id)
        => string.IsNullOrWhiteSpace(id?.Symbol) ? "unknown" : id.Symbol;

}
