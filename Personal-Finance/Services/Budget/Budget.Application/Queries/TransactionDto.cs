namespace Budget.Application.Queries;

public record TransactionDto(
    Guid Id,
    Guid WalletId,
    decimal Amount,
    string Type,
    string Description,
    DateTime Date,
    string Currency,
    string CategoryName
);


