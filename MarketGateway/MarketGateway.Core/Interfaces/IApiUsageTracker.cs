namespace MarketGateway.Interfaces;

public interface IApiUsageTracker
{
    Task<int> GetCallsMadeAsync(string vendor, DateTime dateUtc);
    Task<int> GetCallsLastMinuteAsync(string vendor);
    Task IncrementUsageAsync(string vendor, DateTime dateUtc);
}
