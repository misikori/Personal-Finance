using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Report.Domain.Entities;

namespace Report.Application.Abstractions
{
    public record GenerateTransactionsReportCommand(string UserName, IEnumerable<Transaction> Transactions);
    public class GenerateTransactionReportHandler
    {
        private readonly IPdfReportGenerator _pdfGenerator;

        public GenerateTransactionReportHandler(IPdfReportGenerator pdfGenerator)
        {
            this._pdfGenerator = pdfGenerator ?? throw new ArgumentNullException(nameof(pdfGenerator));
        }

        public async Task<byte[]> HandleAsync(GenerateTransactionsReportCommand command, CancellationToken cancellationToken = default)
        {
            return await _pdfGenerator.GenerateTransactionsReportAsync(command.UserName, command.Transactions);
        }
    }
}
