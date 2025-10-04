using Budget.Application.Transactions;
using Microsoft.AspNetCore.Mvc;

namespace Budget.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController(ITransactionService transactionService) : ControllerBase
    {
        private readonly ITransactionService _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));

        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionDto transactionDto)
        {
            await this._transactionService.CreateTransactionAsync(transactionDto);
            return this.Ok();
        }

    }
}
