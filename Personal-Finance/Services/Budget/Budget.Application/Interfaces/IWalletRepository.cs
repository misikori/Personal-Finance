using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Budget.Domain.Entities;

namespace Budget.Application.Interfaces
{
    
    public interface IWalletRepository
    {
        Task<Wallet?> GetByIdAsync(Guid walletId);
        Task<IEnumerable<Wallet>> GetByUserIdAsync(Guid userId);
        Task AddAsync(Wallet wallet);
        void Update(Wallet wallet);
        Task SaveChangesAsync(); // commit transactions.
    }
}
