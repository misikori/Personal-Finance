using Currency.API.Configuration;
using Currency.API.Hosted;
using Currency.API.Hosted.Fetchers;
using Currency.Common.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddHttpClient<ICurrencyRatesFetcher, CurrencyRatesFetcher>();
builder.Services.AddCurrencyCommonServices(builder.Configuration);

builder.Services.Configure<CurrencyApiSettings>(builder.Configuration.GetSection("CurrencyApi"));

builder.Services.AddHostedService<CurrencyHostedService>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    _ = app.UseSwagger();
    _ = app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
