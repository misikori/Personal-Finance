using MarketGateway.Shared.DTOs;

namespace MarketGateway.Data.Services.Quotes;

public interface IQuoteStorage
{
    Task<QuoteDto?> TryReadAsync(MarketDataRequest req, CancellationToken ct);
    Task SaveAsync(QuoteDto dto, CancellationToken ct);
}