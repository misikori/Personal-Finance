using Budget.Application.Interfaces;
using Budget.Application.RecurringTransactions;
using Budget.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Budget.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecurringTransactionsController(IRecurringTransactionRepository repository) : ControllerBase
    {
        private readonly IRecurringTransactionRepository _repository = repository;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRecurringTransactionDto recurringDto)
        {
            if (!Enum.TryParse(recurringDto.TransactionType, true, out TransactionType transactionType) ||
                !Enum.TryParse(recurringDto.Frequency, true, out RecurrenceFrequency frequency))
            {
                return this.BadRequest("Invalid TransactionType or Frequrency specified.");
            }

            RecurringTransaction recurringTransaction = new()
            {
                UserId = recurringDto.UserId,
                WalletId = recurringDto.WalletId,
                Amount = recurringDto.Amount,
                TransactionType = transactionType,
                Description = recurringDto.Description,
                Category = recurringDto.Category,
                Currency = recurringDto.Currency,
                RecurrenceFrequency = frequency,
                StartDate = recurringDto.StartDate,
                NextDueDate = recurringDto.StartDate,
                EndDate = recurringDto.EndDate,
                IsActive = true
            };

            await this._repository.AddAsync(recurringTransaction);
            await this._repository.SaveChangesAsync();
            return this.Ok(recurringTransaction);
        }

        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetForUser(Guid userId)
        {
            IEnumerable<RecurringTransaction> transactions = await this._repository.GetByUserIdAsync(userId);
            return this.Ok(transactions);
        }
    }
}
