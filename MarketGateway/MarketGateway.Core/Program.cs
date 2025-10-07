using dotenv.net;
using MarketGateway.StartUp;
using Microsoft.AspNetCore.Builder;


var builder = WebApplication.CreateBuilder(args);

DotEnv.Load(options:new DotEnvOptions(probeForEnv:true,overwriteExistingVars:false));

builder.Configuration
    .AddJsonFile("appsettings.Development.json", false, true)
    .AddEnvironmentVariables();

builder.AddSerilogLogging();
builder.Services.AddMarketGateway(builder.Configuration);
builder.Services.AddStorage(builder.Configuration);
builder.Services.AddGrpc();

#if DEBUG
builder.Services.AddGrpcReflection();
#endif

builder.ConfigureKestrelForGrpc();

var app = builder.Build();
app.MapMarketGatewayEndpoints();

#if DEBUG
app.MapGrpcReflectionService();
#endif
await app.InitDatabaseAsync();
app.Run();

