using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Budget.Application.Transactions
{
    public interface ITransactionService
    {
        Task CreateTransactionAsync(CreateTransactionDto transactionDto);
    }
}
