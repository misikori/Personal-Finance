using System.Text.Json;
using System.Text.Json.Serialization;
using MarketGateway.Data.Interfaces;
using MarketGateway.Shared.DTOs;

namespace MarketGateway.Data;

/// <summary>
/// Handles saving raw API responses and metadata by vendor/date/symbol.
/// </summary>

public class FileStorageService : IStorageService
{
    private readonly string _rootPath;
    public FileStorageService(string rootPath = "DataStorage")
    {
        _rootPath = rootPath;
        Directory.CreateDirectory(_rootPath);
    }

    
    public async Task SaveParsedResultAsync(MarketDataResultBase result)
    {
        // Resolve keys
        var safeVendor = Sanitize(result.Vendor);
        var safeType   = Sanitize(result.Type.ToString());
        var safeId = safeVendor+"-"+safeType;
        // Try to pick a meaningful timestamp from the DTO if a "Timestamp" property exists
        var ts = ExtractTimestampUtc(result) ?? DateTime.UtcNow;

        // Folder structure: DataStorage/parsed/<Vendor>/<Type>/YYYY-MM-DD/
        var dateFolder = Path.Combine(_rootPath, "parsed", safeVendor, safeType, ts.ToString("yyyy-MM-dd"));
        Directory.CreateDirectory(dateFolder);

        var fileName = $"{safeId}_{ts:HHmmssfff}.json";
        var filePath = Path.Combine(dateFolder, fileName);

        // Serialize using the actual runtime type (e.g., QuoteDto)
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var json = JsonSerializer.Serialize(result, result.GetType(), options);
        await File.WriteAllTextAsync(filePath, json);

        // Keep a simple metadata file for quick lookups
        await UpdateParsedMetadataAsync(dateFolder, safeId, filePath);
    }

    public async Task SaveApiResponseAsync(string vendor, string identifier, string json, CancellationToken ct = default)
    {
        var safeVendor = Sanitize(vendor);
        var safeName   = Sanitize(identifier);

        var dateFolder = Path.Combine(_rootPath, safeVendor, DateTime.UtcNow.ToString("yyyy-MM-dd"));
        Directory.CreateDirectory(dateFolder);

        var fileName = $"{safeName}_{DateTime.UtcNow:HHmmssfff}.json";
        var filePath = Path.Combine(dateFolder, fileName);

        await File.WriteAllTextAsync(filePath, json, ct);
        await UpdateMetadataAsync(dateFolder, safeName, filePath, ct);
    }

    public async Task<string?> TryReadLatestAsync(string vendor, string identifier, DateTime? dateUtc = null, CancellationToken ct = default)
    {
        var safeVendor = Sanitize(vendor);
        var safeName   = Sanitize(identifier);
        var when       = (dateUtc ?? DateTime.UtcNow).ToString("yyyy-MM-dd");

        var dateFolder = Path.Combine(_rootPath, safeVendor, when);
        if (!Directory.Exists(dateFolder)) return null;

        var prefix = safeName + "_";
        var latest = Directory.EnumerateFiles(dateFolder, "*.json")
            .Where(f => Path.GetFileName(f).StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(Path.GetFileName) // HHmmssfff is lexicographically sortable
            .FirstOrDefault();
        return latest is null ? null : await File.ReadAllTextAsync(latest, ct);
    }
    
    public Task<IEnumerable<string>> GetSavedFilesAsync(string vendor, DateTime dateUtc, CancellationToken ct = default)
    {
        var safeVendor = Sanitize(vendor);
        var folder = Path.Combine(_rootPath, safeVendor, dateUtc.ToString("yyyy-MM-dd"));
        IEnumerable<string> files = Directory.Exists(folder)
            ? Directory.GetFiles(folder, "*.json")
            : Array.Empty<string>();
        return Task.FromResult(files);
    }
    private static async Task UpdateMetadataAsync(string dateFolder, string identifier, string filePath, CancellationToken ct)
    {
        var metaFile  = Path.Combine(dateFolder, "_metadata.json");
        var metadata  = new Dictionary<string, object?>();
        var fileName  = Path.GetFileName(filePath);

        if (File.Exists(metaFile))
        {
            var json = await File.ReadAllTextAsync(metaFile, ct);
            metadata = JsonSerializer.Deserialize<Dictionary<string, object?>>(json) ?? new();
        }

        metadata["lastUpdated"] = DateTime.UtcNow;
        metadata[$"lastFile_{identifier}"] = fileName;
        metadata[$"size_{identifier}"] = new FileInfo(filePath).Length;

        var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await File.WriteAllTextAsync(metaFile, JsonSerializer.Serialize(metadata, options), ct);
    }
    private static string Sanitize(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "unknown";
        var bad = Path.GetInvalidFileNameChars();
        return new string(s.Select(ch => bad.Contains(ch) ? '_' : ch).ToArray());
    }
    
    private static DateTime? ExtractTimestampUtc(MarketDataResultBase result)
    {
        var pi = result.GetType().GetProperty("Timestamp");
        if (pi == null) return null;

        var val = pi.GetValue(result);
        if (val is DateTime dt)
        {
            if (dt.Kind == DateTimeKind.Unspecified) return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            return dt.ToUniversalTime();
        }
        if (val is DateTimeOffset dto) return dto.UtcDateTime;

        return null;
    }
    
    private static async Task UpdateParsedMetadataAsync(string dateFolder, string identifier, string filePath)
    {
        var metaFile  = Path.Combine(dateFolder, "_parsed_metadata.json");
        var metadata  = new Dictionary<string, object?>();
        var fileName  = Path.GetFileName(filePath);

        if (File.Exists(metaFile))
        {
            var json = await File.ReadAllTextAsync(metaFile);
            metadata = JsonSerializer.Deserialize<Dictionary<string, object?>>(json) ?? new();
        }

        metadata["lastUpdated"] = DateTime.UtcNow;
        metadata[$"lastParsedFile_{identifier}"] = fileName;
        metadata[$"parsedSize_{identifier}"] = new FileInfo(filePath).Length;

        var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await File.WriteAllTextAsync(metaFile, JsonSerializer.Serialize(metadata, options));
    }
}
