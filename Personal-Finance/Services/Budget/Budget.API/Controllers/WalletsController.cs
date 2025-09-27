using Budget.Application.Interfaces;
using Budget.Application.Wallets;
using Microsoft.AspNetCore.Mvc;

namespace Budget.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WalletsController(IWalletService walletService, IWalletRepository walletRepository) : ControllerBase
    {
        private readonly IWalletService _walletService = walletService ?? throw new ArgumentNullException(nameof(walletService));
        private readonly IWalletRepository _walletRepository = walletRepository ?? throw new ArgumentNullException(nameof(walletRepository));

        [HttpPost]
        public async Task<IActionResult> CreateWallet([FromBody] CreateWalletDto walletDto)
        {
            Domain.Entities.Wallet wallet = await this._walletService.CreateWalletAsync(walletDto);
            return this.CreatedAtAction(nameof(GetWalletById), new { walletId = wallet.Id }, wallet);
        }

        [HttpGet("{walletId:guid}")]
        public async Task<IActionResult> GetWalletById(Guid walletId)
        {
            Domain.Entities.Wallet? wallet = await this._walletRepository.GetByIdAsync(walletId);
            return wallet == null ? this.NotFound() : this.Ok(wallet);
        }

        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetWalletsByUserId(Guid userId)
        {
            IEnumerable<Domain.Entities.Wallet> wallets = await this._walletRepository.GetByUserIdAsync(userId);
            return this.Ok(wallets);
        }
    }
}
