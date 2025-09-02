using MarketGateway.Grpc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace MarketGateway.StartUp;


public static class EndpointRouteBuilderExtensions
{
    public static void MapMarketGatewayEndpoints(this WebApplication app)
    {
        app.MapGrpcService<MarketDataGatewayService>();
        app.MapGet("/", () => "MarketData gRPC is running on :5288");
        app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
    }
}