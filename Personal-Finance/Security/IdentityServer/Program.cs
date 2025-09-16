using IdentityServer.Entities;
using IdentityServer.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
//builder.Services.AddAuthentication();

builder.Services.ConfigureJWT(builder.Configuration);
builder.Services.ConfigurePersistence(builder.Configuration);
builder.Services.ConfigureIdentity();
builder.Services.ConfigureMiscellaneousServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("CorsPolicy");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();