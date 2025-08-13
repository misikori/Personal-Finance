using MarketGateway;
using MarketGateway.Data;
using Microsoft.EntityFrameworkCore;



var builder = Host.CreateApplicationBuilder(args);
//builder.Services.AddHostedService<Worker>();
// Use SQLite for now
builder.Services.AddDbContext<MarketDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("MarketDb"),
        sqliteOptions => sqliteOptions.MigrationsAssembly("MarketGateway.Data"))
);
var host = builder.Build();

await AlphaVantageTester.RunTestAsync();
host.Run();