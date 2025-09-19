
namespace Budget.Domain.Entities
{
    public class RecurringTransaction
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public Guid WalletId { get; set; }
        public decimal Amount { get; set; }
        public TransactionType TransactionType { get; set; }
        public string? Description { get; set; }
        public required string Currency {  get; set; }

        public RecurrenceFrequency RecurrenceFrequency { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime NextDueDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
