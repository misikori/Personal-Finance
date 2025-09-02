namespace MarketGateway.Interfaces;

/// <summary>
/// Tracks vendor API usage for rate-limit enforcement and reporting.
/// Implementations should be thread-safe.
/// </summary>
public interface IApiUsageTracker
{
    /// <summary>
    /// Total calls recorded for a vendor on a specific UTC date (YYYY-MM-DD).
    /// </summary>
    Task<int> GetCallsMadeAsync(string vendor, DateTime dateUtc, CancellationToken ct = default);

    /// <summary>
    /// Record a successful call occurrence. Implementations may coalesce/flush asynchronously.
    /// </summary>
    Task IncrementUsageAsync(string vendor, DateTime timestampUtc, CancellationToken ct = default);
    
}