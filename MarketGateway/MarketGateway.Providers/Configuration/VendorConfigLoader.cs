using MarketGateway.Shared.Configuration;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MarketGateway.StartUp;

public static class VendorConfigLoader
{
    public static IReadOnlyList<VendorConfig> LoadFromFolder(string folder)
    {
        var files = Directory.GetFiles(folder, "*.yaml", SearchOption.AllDirectories);
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(NullNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        var list = new List<VendorConfig>();
        foreach (var file in files)
        {
            var yaml = File.ReadAllText(file);
            var cfg  = deserializer.Deserialize<VendorConfig>(yaml);
            if (!string.IsNullOrWhiteSpace(cfg.Vendor))
                list.Add(cfg);
        }
        return list;
    }
}