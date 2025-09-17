using Microsoft.AspNetCore.Mvc;
using Budget.Application.Transactions;

namespace Budget.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionDto transactionDto)
        {
            await _transactionService.CreateTransactionAsync(transactionDto);
            return Ok();
        }

    }
}
