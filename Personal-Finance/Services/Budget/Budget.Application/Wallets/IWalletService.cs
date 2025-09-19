using Budget.Domain.Entities;

namespace Budget.Application.Wallets
{
    public interface IWalletService
    {
        Task<Wallet> CreateWalletAsync(CreateWalletDto walletDto);
    }
}
