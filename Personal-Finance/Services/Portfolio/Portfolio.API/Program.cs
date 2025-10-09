using Portfolio.Core.Repositories;
using Portfolio.Core.Services;
using Portfolio.Data;
using Portfolio.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure SQL Server Database
var connectionString = builder.Configuration.GetConnectionString("PortfolioDb")
    ?? "Server=127.0.0.1,1433;Database=PortfolioDb;User Id=sa;Password=MATF12345678rs2;TrustServerCertificate=True;Encrypt=False;";

builder.Services.AddDbContext<PortfolioDbContext>(options =>
    options.UseSqlServer(connectionString));

// Swagger/OpenAPI configuration with JWT support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Portfolio API",
        Version = "v1",
        Description = "Portfolio management service with ML predictions, multi-currency support, and full Budget service integration.",
        Contact = new OpenApiContact
        {
            Name = "Portfolio Service",
            Email = "portfolio@personalfinance.com"
        }
    });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Enable XML comments for Swagger documentation
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Configure JWT Authentication (integrate with IdentityServer)
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "MyVerySecretSecretSecretMessage1";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "Personal-Finance Identity",
        ValidAudience = jwtSettings["Audience"] ?? "Personal-Finance",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();

// Configure MarketGateway URL from appsettings
var marketGatewayUrl = builder.Configuration.GetValue<string>("MarketGatewayUrl") 
    ?? throw new InvalidOperationException("MarketGatewayUrl must be configured in appsettings.json");

// Configure Budget service URL from appsettings
var budgetServiceUrl = builder.Configuration.GetValue<string>("BudgetServiceUrl") 
    ?? throw new InvalidOperationException("BudgetServiceUrl must be configured in appsettings.json");

// Configure Currency service URL from appsettings
var currencyServiceUrl = builder.Configuration.GetValue<string>("CurrencyServiceUrl") 
    ?? throw new InvalidOperationException("CurrencyServiceUrl must be configured in appsettings.json");

// Configure IdentityServer URL from appsettings
var identityServerUrl = builder.Configuration.GetValue<string>("IdentityServerUrl") 
    ?? throw new InvalidOperationException("IdentityServerUrl must be configured in appsettings.json");

// Register repositories (use SQL Server implementations from Portfolio.Data)
builder.Services.AddScoped<IPortfolioRepository, Portfolio.Data.Repositories.PortfolioRepository>();

// Register Currency service gRPC client
builder.Services.AddSingleton(sp =>
{
    var channel = Grpc.Net.Client.GrpcChannel.ForAddress(currencyServiceUrl);
    return new Currency.grpc.CurrencyRatesProtoService.CurrencyRatesProtoServiceClient(channel);
});

// Register IdentityServer HTTP client for user resolution
builder.Services.AddHttpClient<IUserService, UserService>(client =>
{
    client.BaseAddress = new Uri(identityServerUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
});

// Register services
builder.Services.AddSingleton<IMarketDataService>(sp => 
    new MarketDataService(
        marketGatewayUrl, 
        sp.GetRequiredService<ILogger<MarketDataService>>()
    ));
builder.Services.AddScoped<ICurrencyConverter, CurrencyConverterService>();
builder.Services.AddScoped<IPortfolioService, PortfolioService>();
builder.Services.AddScoped<IPredictionService, PredictionService>();

// Budget service integration via gRPC (with username → userId resolution)
builder.Services.AddScoped<IBudgetService>(sp => 
    new BudgetServiceClient(
        budgetServiceUrl,
        sp.GetRequiredService<IUserService>(),
        sp.GetRequiredService<ILogger<BudgetServiceClient>>()
    ));

// Add CORS (if needed for frontend)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Apply database migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<PortfolioDbContext>();
        dbContext.Database.Migrate();
        app.Logger.LogInformation("Portfolio database migrated successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred while migrating the Portfolio database");
        throw;
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Portfolio API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowAll");
app.UseAuthentication(); // Add authentication middleware
app.UseAuthorization();
app.MapControllers();

app.Run();
