namespace Budget.Application.Transactions
{
    public record CreateTransactionDto(
        Guid UserId,
        Guid WalletId,
        decimal Amount,
        string Type,
        string Description,
        DateTime Date,
        string Currency
    );


}
