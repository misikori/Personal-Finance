using Budget.Application.Interfaces;
using Budget.Domain.Entities;

namespace Budget.Application.Wallets
{
    public class WalletService(IWalletRepository walletRepo) : IWalletService
    {
        private readonly IWalletRepository _walletRepo = walletRepo;

        public async Task<Wallet> CreateWalletAsync(CreateWalletDto walletDto)
        {
            Wallet newWallet = new()
            {
                UserId = walletDto.UserId,
                Name = walletDto.Name,
                Currency = walletDto.Currency,
                CurrentBalance = 0
            };

            await this._walletRepo.AddAsync(newWallet);
            await this._walletRepo.SaveChangesAsync();

            return newWallet;
        }
    }
}
