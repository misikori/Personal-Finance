using Budget.Application.Interfaces;
using Budget.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Budget.Infrastructure.Persistence.Repositories
{
    public class WalletRepository(BudgetDbContext context) : IWalletRepository
    {
        private readonly BudgetDbContext _context = context;

        public async Task<Wallet?> GetByIdAsync(Guid walletId) => await this._context.Wallets.FindAsync(walletId);

        public async Task<IEnumerable<Wallet>> GetByUserIdAsync(Guid userId) => await this._context.Wallets
                .Where(w => w.UserId == userId)
                .ToListAsync();

        public async Task AddAsync(Wallet wallet) => _ = await this._context.Wallets.AddAsync(wallet);

        public void Update(Wallet wallet) => _ = this._context.Wallets.Update(wallet);

        public async Task SaveChangesAsync() => _ = await this._context.SaveChangesAsync();

    }
}
