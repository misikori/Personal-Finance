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
    ?? "Server=localhost;Database=PortfolioDb;Trusted_Connection=True;TrustServerCertificate=True;";

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
        Description = "Portfolio management service with SQL Server, budget tracking, and JWT authentication",
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
var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyForJWTTokenGeneration123!";

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
        ValidIssuer = jwtSettings["Issuer"] ?? "IdentityServer",
        ValidAudience = jwtSettings["Audience"] ?? "PortfolioAPI",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();

// Configure MarketGateway URL from appsettings
var marketGatewayUrl = builder.Configuration.GetValue<string>("MarketGatewayUrl") 
    ?? "http://localhost:5288";

// Register repositories (use SQL Server implementations from Portfolio.Data)
builder.Services.AddScoped<IPortfolioRepository, Portfolio.Data.Repositories.PortfolioRepository>();

// Register services
builder.Services.AddSingleton<IMarketDataService>(sp => 
    new MarketDataService(
        marketGatewayUrl, 
        sp.GetRequiredService<ILogger<MarketDataService>>()
    ));
builder.Services.AddScoped<IPortfolioService, PortfolioService>();
builder.Services.AddScoped<IPredictionService, PredictionService>();

// TODO: Replace with actual Budget service client when Budget service is integrated
// This placeholder currently does nothing (always returns true)
builder.Services.AddScoped<IBudgetService, BudgetServicePlaceholder>();

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

// Apply database migrations automatically
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
        c.RoutePrefix = string.Empty; // Serve Swagger UI at the app's root
    });
}

app.UseCors("AllowAll");
app.UseAuthentication(); // Add authentication middleware
app.UseAuthorization();
app.MapControllers();

var port = builder.Configuration.GetValue<int>("Port", 5100);
app.Run($"http://localhost:{port}");
