using MarketGateway.Interfaces;

namespace MarketGateway.Tests.TestHelpers;

public sealed class InMemoryUsageTracker : IApiUsageTracker
{
    public int PerMinute { get; set; }
    public int PerDay { get; set; }

    public Task<int> GetCallsMadeAsync(string vendor, DateTime date) => Task.FromResult(PerDay);
    public Task<int> GetCallsLastMinuteAsync(string vendor) => Task.FromResult(PerMinute);
    public Task IncrementUsageAsync(string vendor, DateTime utcNow)
    {
        PerMinute++; PerDay++;
        return Task.CompletedTask;
    }
}