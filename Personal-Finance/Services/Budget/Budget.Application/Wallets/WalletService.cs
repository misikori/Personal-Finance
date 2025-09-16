using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Budget.Domain.Entities;
using Budget.Application.Interfaces;

namespace Budget.Application.Wallets
{
    public class WalletService : IWalletService
    {
        private readonly IWalletRepository _walletRepo;

        public WalletService(IWalletRepository walletRepo)
        {
            _walletRepo = walletRepo;
        }

        public async Task<Wallet> CreateWalletAsync(CreateWalletDto dto)
        {
            var newWallet = new Wallet
            {
                UserId = dto.UserId,
                Name = dto.Name,
                Currency = dto.Currency,
                CurrentBalance = 0
            };

            await _walletRepo.AddAsync(newWallet);
            await _walletRepo.SaveChangesAsync();

            return newWallet;
        }
    }
}
