namespace Budget.Application.SpendingLimits;

public record CreateLimitDto(
    Guid WalletId,
    string CategoryName,
    decimal Amount,
    int Month,
    int Year,
    Guid UserId
);

