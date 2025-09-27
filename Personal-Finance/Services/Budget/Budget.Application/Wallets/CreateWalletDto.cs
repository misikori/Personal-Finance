namespace Budget.Application.Wallets
{
    public record CreateWalletDto(
        Guid UserId,
        string Name,
        string Currency
        );
}
