using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace MarketGateway.StartUp;

public static class KestrelExtensions
{
    public static void ConfigureKestrelForGrpc(this WebApplicationBuilder builder)
    {
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(5288, o =>
            {
                o.Protocols = HttpProtocols.Http2;
            });
            
            options.ListenAnyIP(8080, listen =>
            {
                listen.Protocols = HttpProtocols.Http1;
            });
        });
    }
}