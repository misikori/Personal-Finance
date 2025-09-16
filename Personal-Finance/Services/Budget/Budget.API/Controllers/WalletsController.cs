using Microsoft.AspNetCore.Mvc;
using Budget.Application.Interfaces;
using Budget.Application.Wallets;

namespace Budget.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WalletsController : ControllerBase
    {
        private readonly IWalletService _walletService;
        private readonly IWalletRepository _walletRepository;

        public WalletsController(IWalletService walletService, IWalletRepository walletRepository)
        {
            _walletService = walletService;
            _walletRepository = walletRepository;
        }

        [HttpPost]
        public async Task<IActionResult> CreateWallet([FromBody] CreateWalletDto dto)
        {
            var wallet = await _walletService.CreateWalletAsync(dto);
            return CreatedAtAction(nameof(GetWalletById), new { walletId = wallet.Id }, wallet);
        }

        [HttpGet("{walletId:guid}")]
        public async Task<IActionResult> GetWalletById(Guid walletId)
        {
            var wallet = await _walletRepository.GetByIdAsync(walletId);
            return wallet == null ? NotFound() : Ok(wallet);
        }

        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetWalletsByUserId(Guid userId)
        {
            var wallets = await _walletRepository.GetByUserIdAsync(userId);
            return Ok(wallets);
        }
    }
}
