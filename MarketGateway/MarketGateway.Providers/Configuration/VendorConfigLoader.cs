using Microsoft.Extensions.Logging;
using Serilog;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MarketGateway.Providers.Configuration;

public static class VendorConfigLoader
{
    public static IReadOnlyList<VendorConfig> LoadFromFolder(string folder)
    {
        if (!Directory.Exists(folder))
            throw new DirectoryNotFoundException($"Vendor config folder not found: {folder}");

        
        var files = Directory.GetFiles(folder, "*.yaml", SearchOption.AllDirectories);
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(NullNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
            
        Log.Information("VendorConfigLoader: found {Count} file(s): {Files}",
            files.Length, string.Join(", ", files.Select(Path.GetFileName)));

        var list = new List<VendorConfig>();
        foreach (var file in files)
        {
            var yaml = File.ReadAllText(file);
            var cfg  = deserializer.Deserialize<VendorConfig>(yaml);
            if (!string.IsNullOrWhiteSpace(cfg.Vendor))
                list.Add(cfg);
                Log.Information("Parsed vendor {Vendor} with {N} endpoint(s). Types: {Types}",
                    cfg.Vendor, cfg.Endpoints.Count,
                    string.Join(", ", cfg.Endpoints.Values.Select(e => e.DataType)));
        }
        return list;
    }
}