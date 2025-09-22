using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Budget.Application.Wallets
{
    public record CreateWalletDto(
        Guid UserId,
        string Name,
        string Currency
        );
}
