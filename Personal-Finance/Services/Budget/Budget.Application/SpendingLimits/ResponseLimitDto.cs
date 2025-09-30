namespace Budget.Application.SpendingLimits;

public record ResponseLimitDto(
    Guid Id,
    decimal Amount,
    int Month,
    int Year,
    string CategoryName
);
