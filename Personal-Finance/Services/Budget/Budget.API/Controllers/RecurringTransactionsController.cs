using Budget.Application.Interfaces;
using Budget.Application.RecurringTransactions;
using Budget.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Budget.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecurringTransactionsController : ControllerBase
    {
        private readonly IRecurringTransactionRepository _repository;

        public RecurringTransactionsController(IRecurringTransactionRepository repository)
        {
            _repository = repository;

        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRecurringTransactionDto recurringDto)
        {
            if (!Enum.TryParse<TransactionType>(recurringDto.TransactionType, true, out var transactionType) ||
                !Enum.TryParse<RecurrenceFrequency>(recurringDto.Frequency, true, out var frequency))
                {
                return BadRequest("Invalid TransactionType or Frequrency specified.");
            }

            var recurringTransaction = new RecurringTransaction
            {
                UserId = recurringDto.UserId,
                WalletId = recurringDto.WalletId,
                Amount = recurringDto.Amount,
                TransactionType = transactionType,
                Description = recurringDto.Description,
                Currency = recurringDto.Currency,
                RecurrenceFrequency = frequency,
                StartDate = recurringDto.StartDate,
                NextDueDate = recurringDto.StartDate,
                EndDate = recurringDto.EndDate,
                IsActive = true
            };

            await _repository.AddAsync(recurringTransaction);
            await _repository.SaveChangesAsync();
            return Ok(recurringTransaction);
        }

        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetForUser(Guid userId)
        {
            var transactions = await _repository.GetByUserIdAsync(userId);
            return Ok(transactions);
        }
    }
}
