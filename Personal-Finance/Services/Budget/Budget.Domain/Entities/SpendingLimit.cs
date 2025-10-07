namespace Budget.Domain.Entities;

public class SpendingLimit
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public decimal Amount { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public required string CategoryName { get; set; }
}
