using EventBus.Messages.Constants;
using MassTransit;
using QuestPDF.Infrastructure;
using Report.Application.Abstractions;
using Report.Infrastructure.Messaging;
using Report.Infrastructure.Pdf;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;


// Add services to the container.

builder.Services.AddScoped<IPdfReportGenerator, QuestPdfReportGenerator>();
builder.Services.AddScoped<GenerateTransactionReportHandler>();
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<TransactionsReportConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["EventBusSettings:HostAddress"]);

        cfg.ReceiveEndpoint(EventBusConstants.TansactionsReportQueue, e =>
        {
            e.ConfigureConsumer<TransactionsReportConsumer>(context);
        });
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
