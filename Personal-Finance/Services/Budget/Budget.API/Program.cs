using Budget.Application.Categories;
using Budget.Application.Interfaces;
using Budget.Application.SpendingLimits;
using Budget.Application.Transactions;
using Budget.Application.Wallets;
using Budget.Infrastructure.ExternalServices;
using Budget.Infrastructure.Persistence;
using Budget.Infrastructure.Persistence.Repositories;
using Currency.grpc;
using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// add sqlite
string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<BudgetDbContext>(options =>
    options.UseSqlite(connectionString));

// Add services to the container.
builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IRecurringTransactionRepository, RecurringTransactionRepository>();
builder.Services.AddScoped<ICurrencyConverter, GrpcCurrencyConverter>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ISpendingLimitRepository, SpendingLimitRepository>();
builder.Services.AddScoped<ISpendingLimitService, SpendingLimitService>();
builder.Services.AddSingleton(sp =>
{
    string? currencyServiceUrl = builder.Configuration["ServiceUrls:CurrencyService"];
    GrpcChannel channel = GrpcChannel.ForAddress(currencyServiceUrl!);
    return new CurrencyRatesProtoService.CurrencyRatesProtoServiceClient(channel);
});

// builder.Services.AddGrpcClient<CurrencyRatesProtoService.CurrencyRatesProtoServiceClient>(options =>
// {
//     var currencyServiceUrl = builder.Configuration["ServiceUrls:CurrencyService"];
//     options.Address = new Uri(currencyServiceUrl!);
// });

builder.Services.AddControllers();
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
