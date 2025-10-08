namespace Budget.Application.RecurringTransactions
{
    public record CreateRecurringTransactionDto(
        Guid UserId,
        Guid WalletId,
        decimal Amount,
        string TransactionType,
        string Description,
        string Currency,
        string Frequency,
        DateTime StartDate,
        DateTime? EndDate
    );

}
