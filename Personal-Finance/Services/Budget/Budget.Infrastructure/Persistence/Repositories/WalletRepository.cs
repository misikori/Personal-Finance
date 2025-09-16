using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Budget.Application.Interfaces;
using Budget.Domain.Entities;
using Budget.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Budget.Infrastructure.Persistence.Repositories
{
    public class WalletRepository : IWalletRepository
    {
        private readonly BudgetDbContext _context;

        public WalletRepository(BudgetDbContext context)
        {
            _context = context;
        }

        public async Task<Wallet?> GetByIdAsync(Guid walletId)
        {
            return await _context.Wallets.FindAsync(walletId);
        }

        public async Task<IEnumerable<Wallet>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Wallets
                .Where(w => w.UserId == userId)
                .ToListAsync();
        }

        public async Task AddAsync(Wallet wallet)
        {
            await _context.Wallets.AddAsync(wallet);
        }

        public void Update(Wallet wallet)
        {
            _context.Wallets.Update(wallet);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

    }
}
