using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Serilog;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MarketGateway.Providers.Configuration;

public static class VendorConfigLoader
{
    
    private static readonly Regex EnvToken =
        new(@"\$\{([A-Za-z_][A-Za-z0-9_]*)(?::([^}]*))?\}", RegexOptions.Compiled);
        
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

            yaml = ResolveEnvPlaceholders(yaml);
            Log.Information("Config: {yaml}", yaml);
            var cfg  = deserializer.Deserialize<VendorConfig>(yaml);
            Log.Debug("VendorConfigLoader: loading vendor config");
            Log.Information("Config: {cfg}", cfg.ApiKey);
            cfg.Validate();
            if (!string.IsNullOrWhiteSpace(cfg.Vendor))
                list.Add(cfg);
            Log.Information("Parsed vendor {Vendor} with {N} endpoint(s). Types: {Types}",
                cfg.Vendor, cfg.Endpoints.Count,
                string.Join(", ", cfg.Endpoints.Values.Select(e => e.DataType)));
        }
        return list;
    }

    private static string ResolveEnvPlaceholders(string text)
    {
        return EnvToken.Replace(text, m =>
        {
            var name = m.Groups[1].Value;
            var def = m.Groups[2].Success ? m.Groups[2].Value : string.Empty;
            var val = Environment.GetEnvironmentVariable(name);
            return string.IsNullOrWhiteSpace(val) ? def : val;
        });
    }
}