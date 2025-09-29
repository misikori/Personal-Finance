using MarketGateway.Grpc;
using MarketGateway.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace MarketGateway.StartUp;


public static class EndpointRouteBuilderExtensions
{
    public static void MapMarketGatewayEndpoints(this WebApplication app)
    {
        app.MapGrpcService<MarketDataGatewayService>();
        app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
        app.MapGet("/ready", (IMarketDataBroker _) =>
        {
            return Task.FromResult(Results.Ok(new { ready = true }));
        });
        
        if (app.Environment.IsDevelopment())
        {
            app.MapGet("/", () => "MarketData gRPC is running");
        }
    }
}