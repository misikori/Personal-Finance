using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventBus.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using Report.Application.Abstractions;
using Report.Domain.Entities;

namespace Report.Infrastructure.Messaging
{
    public class TransactionsReportConsumer : IConsumer<TransactionsReportEvent>
    {
        private readonly GenerateTransactionReportHandler _handler;
        private readonly ILogger<TransactionsReportConsumer> _logger;

        public TransactionsReportConsumer(GenerateTransactionReportHandler handler, ILogger<TransactionsReportConsumer> logger)
        {
            this._handler = handler ?? throw new ArgumentNullException(nameof(handler));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Consume(ConsumeContext<TransactionsReportEvent> context)
        {
            var message = context.Message;

            var transactions = message.TransactionItems.Select(t => new Transaction(t.Amount, t.WalletName, t.Description, t.Date, t.Currency,
                t.CategoryName, t.TransactionType == TransactionType.Income));

            var pdfBytes = await _handler.HandleAsync(new GenerateTransactionsReportCommand(message.UserName, transactions));

            var folder = Path.Combine(AppContext.BaseDirectory, "reports");
            Directory.CreateDirectory(folder);

            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
            // Path.GetInvalidFileNameChars() returns an array of invalid characters for file names so we can protect ourselves
            // We eliminate those characters from the username so we can add it to the file name
            var safeUserName = string.Join("", message.UserName.Split(Path.GetInvalidFileNameChars()));
            safeUserName = safeUserName.Replace(" ", "").Trim();
            var fileName = $"Report_{safeUserName}_{timestamp}.pdf";
            var path = Path.Combine(folder, fileName);

            await File.WriteAllBytesAsync(path, pdfBytes);
            _logger.LogInformation("✅ Report generated for {UserId}. Saved to {Path}", message.UserId, path);
        }
    }
}
