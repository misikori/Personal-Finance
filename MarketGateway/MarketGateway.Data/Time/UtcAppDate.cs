using MarketGateway.Data.Interfaces;

namespace MarketGateway.Data.Time;

public sealed class UtcAppDate : IAppDate
{
    public DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);
}