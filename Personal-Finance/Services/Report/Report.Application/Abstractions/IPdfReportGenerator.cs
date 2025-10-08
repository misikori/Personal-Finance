using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Report.Domain.Entities;

namespace Report.Application.Abstractions
{
    public interface IPdfReportGenerator
    {
        Task<byte[]> GenerateTransactionsReportAsync(string userName, IEnumerable<Transaction> transactions);
    }
}
