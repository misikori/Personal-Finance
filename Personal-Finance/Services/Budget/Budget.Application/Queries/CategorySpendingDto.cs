namespace Budget.Application.Queries;

public record CategorySpendingDto(
    string CategoryName,
    decimal TotalAmount
);
