using MarketGateway.StartUp;
using Microsoft.AspNetCore.Builder;


var builder = WebApplication.CreateBuilder(args);

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

