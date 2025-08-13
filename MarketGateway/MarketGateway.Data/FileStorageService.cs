using System.Text.Json;

namespace MarketGateway.Data;

/// <summary>
/// Handles saving raw API responses and metadata by vendor/date/symbol.
/// </summary>
public class FileStorageService
{
    private readonly string _rootPath;

    public FileStorageService(string rootPath = "DataStorage")
    {
        _rootPath = rootPath;
        Directory.CreateDirectory(_rootPath);
    }

    public async Task SaveApiResponseAsync(string vendor, string identifier, string json)
    {
        var dateFolder = Path.Combine(_rootPath, vendor, DateTime.UtcNow.ToString("yyyy-MM-dd"));
        Directory.CreateDirectory(dateFolder);

        var safeName = identifier.Replace("/", "_").Replace("\\", "_");
        var fileName = $"{safeName}_{DateTime.UtcNow:HHmmss}.json";
        var filePath = Path.Combine(dateFolder, fileName);

        await File.WriteAllTextAsync(filePath, json);
        await UpdateMetadataAsync(vendor, dateFolder, safeName, filePath);
    }

    private async Task UpdateMetadataAsync(string vendor, string dateFolder, string identifier, string filePath)
    {
        var metaFile = Path.Combine(dateFolder, "_metadata.json");
        var metadata = new Dictionary<string, object>();

        if (File.Exists(metaFile))
        {
            var json = await File.ReadAllTextAsync(metaFile);
            metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new();
        }

        metadata["LastUpdated"] = DateTime.UtcNow;
        metadata[$"LastFile_{identifier}"] = Path.GetFileName(filePath);
        metadata[$"Size_{identifier}"] = new FileInfo(filePath).Length;

        await File.WriteAllTextAsync(metaFile, JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true }));
    }

    public IEnumerable<string> GetSavedFiles(string vendor, DateTime date)
    {
        var dateFolder = Path.Combine(_rootPath, vendor, date.ToString("yyyy-MM-dd"));
        return Directory.Exists(dateFolder)
            ? Directory.GetFiles(dateFolder, "*.json")
            : Array.Empty<string>();
    }
}
