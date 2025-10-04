namespace Budget.Application.Queries;

public record TransactionDto(
    Guid Id,
    decimal Amount,
    string Type,
    string Description,
    DateTime Date,
    string Currency,
    string CategoryName
);


