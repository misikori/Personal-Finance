using MarketGateway.Data.Interfaces;
using MarketGateway.Interfaces;

namespace MarketGateway.Tests.TestHelpers;

public sealed class InMemoryUsageTracker : IApiUsageTracker
{
    public int PerDay { get; set; }

    public Task<int> GetCallsMadeAsync(string vendor, DateTime dateUtc, CancellationToken ct = default)
        => Task.FromResult(PerDay);

    public Task IncrementUsageAsync(string vendor, DateTime timestampUtc, CancellationToken ct = default)
    {
        PerDay++; // keep it simple for tests
        return Task.CompletedTask;
    }
}