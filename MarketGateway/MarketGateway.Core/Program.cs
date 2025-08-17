using MarketGateway;
using MarketGateway.Data;
using MarketGateway.Interfaces;
using MarketGateway.Providers;
using MarketGateway.Shared.Configuration;
using Microsoft.EntityFrameworkCore;
using Serilog;
using YamlDotNet.Core.Tokens;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;


var builder = Host.CreateApplicationBuilder(args);
//builder.Services.AddHostedService<Worker>();
// Use SQLite for now
builder.Services.AddDbContext<MarketDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("MarketDb"),
        sqliteOptions => sqliteOptions.MigrationsAssembly("MarketGateway.Data"))
);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/market.log")
    .Enrich.FromLogContext()
    .CreateLogger();

static IReadOnlyList<VendorConfig> LoadVendors(string folder)
{
    var files = Directory.GetFiles(folder, "*.yaml",SearchOption.AllDirectories);
    var deserializer = new DeserializerBuilder()
        .WithNamingConvention(NullNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();
    
    var list = new List<VendorConfig>();
    foreach (var file in files)
    {
        var yaml = File.ReadAllText(file);
        var cfg = deserializer.Deserialize<VendorConfig>(yaml);
        if(!string.IsNullOrWhiteSpace(cfg.Vendor))
            list.Add(cfg);
    }
    return list;
}
var vendors = LoadVendors("../MarketGateway.Shared/Configuration/Vendors");
foreach (var conf in vendors)
{
    var clientName = $"vendor:{conf.Vendor}";

    builder.Services.AddHttpClient(clientName, client =>
    {
        client.BaseAddress = new Uri(conf.BaseUrl, UriKind.Absolute);
        client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip");
        client.Timeout = TimeSpan.FromSeconds(10);
    });
    
    builder.Services.AddSingleton<IMarketDataProviderResolver>(sp =>
    {
        var httpFactory   = sp.GetRequiredService<IHttpClientFactory>();
        var tracker       = sp.GetRequiredService<IApiUsageTracker>();
        var storage       = sp.GetRequiredService<FileStorageService>();
        var time          = sp.GetRequiredService<TimeProvider>();
        var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

        var dict = new Dictionary<string, IMarketDataProvider>(StringComparer.OrdinalIgnoreCase);
        foreach (var cfg in vendors)
        {
            var provider = new VendorMarketDataProvider(
                cfg,
                httpFactory,                                // factory, not client
                tracker,
                storage,
                loggerFactory.CreateLogger<VendorMarketDataProvider>());
            dict[cfg.Vendor] = provider;
        }
        return new MarketDataProviderResolver(dict);
    });
}


var host = builder.Build();

host.Run();