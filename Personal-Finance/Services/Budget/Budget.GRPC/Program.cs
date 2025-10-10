using Budget.Application.Categories;
using Budget.Application.Interfaces;
using Budget.Application.SpendingLimits;
using Budget.Application.Transactions;
using Budget.Application.Wallets;
using Budget.GRPC.Services;
using Budget.Infrastructure.ExternalServices;
using Budget.Infrastructure.Persistence;
using Budget.Infrastructure.Persistence.Repositories;
using Currency.grpc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<BudgetDbContext>(options =>
    options.UseNpgsql(connectionString));
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);


builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ISpendingLimitRepository, SpendingLimitRepository>();

builder.Services.AddScoped<IWalletService,  WalletService>();
builder.Services.AddScoped<ITransactionService,  TransactionService>();
builder.Services.AddScoped<ICategoryService,  CategoryService>();
builder.Services.AddScoped<ISpendingLimitService, SpendingLimitService>();

builder.Services.AddScoped<ICurrencyConverter, GrpcCurrencyConverter>();

var currencyServiceUrl = builder.Configuration["ServiceUrls:CurrencyService"]
    ?? throw new InvalidOperationException("ServiceUrls:CurrencyService must be configured in appsettings.json");

builder.Services.AddGrpcClient<CurrencyRatesProtoService.CurrencyRatesProtoServiceClient>(options =>
{
    options.Address = new Uri(currencyServiceUrl);
});
// Add services to the container.
builder.Services.AddGrpc();

var app = builder.Build();

// Run migrations on startup in Development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<BudgetDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
app.MapGrpcService<BudgetGrpcService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
