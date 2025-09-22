using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Budget.Application.Transactions
{
    public record CreateTransactionDto(
        Guid UserId,
        Guid WalletId,
        decimal Amount,
        string Type,
        string Description,
        DateTime Date,
        string Currency
    );
    
        
}
