using System.Collections.Concurrent;
using MarketGateway.Data;
using MarketGateway.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MarketGateway.Services;


/// <summary>
/// Tracks vendor API usage (per-day persisted, per-minute rolling in-memory).
/// Thread-safe for typical web/service usage; DB updates use atomic UPDATE when possible.
/// </summary>
public class ApiUsageTracker : IApiUsageTracker
{
    private readonly MarketDbContext _dbContext;
    private readonly ConcurrentDictionary<string, (DateTime Date, int DailyCalls)> _dailyCache = new();
    private readonly ConcurrentDictionary<string, ConcurrentQueue<DateTime>> _minuteWindows = new();

    public ApiUsageTracker(MarketDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Gets the number of calls made for a vendor on the given UTC date.
    /// Cache-first, then DB.
    /// </summary>
    public async Task<int> GetCallsMadeAsync(string vendor, DateTime dateUtc, CancellationToken ct = default)
    {
        var dateKey = dateUtc.Date;
        var cacheKey = $"{vendor}:{dateKey:yyyy-MM-dd}";

        if (_dailyCache.TryGetValue(cacheKey, out var cached) && cached.Date == dateKey)
            return cached.DailyCalls;
        
        var usage = await _dbContext.ApiUsages
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Vendor == vendor && u.Date == dateKey, ct)
            .ConfigureAwait(false);

        var calls = usage?.CallsMade ?? 0;
        _dailyCache[cacheKey] = (dateKey, calls);
        return calls;
    }

    /// <summary>
    /// Increments the usage counter for the vendor and date (DB + cache).
    /// Also tracks a rolling per-minute counter in memory (useful for local gating).
    /// </summary>
    public async Task IncrementUsageAsync(string vendor, DateTime timestampUtc, CancellationToken ct = default)
    {
        var dateKey = timestampUtc.Date;
        var cacheKey = $"{vendor}:{dateKey:yyyy-MM-dd}";
        
        var affected = await _dbContext.ApiUsages
            .Where(u => u.Vendor == vendor && u.Date == dateKey)
            .ExecuteUpdateAsync(setters => setters.SetProperty(u => u.CallsMade, u => u.CallsMade + 1), ct)
            .ConfigureAwait(false);

        if (affected == 0)
        {
            _dbContext.ApiUsages.Add(new ApiUsage
            {
                Vendor = vendor,
                Date = dateKey,
                CallsMade = 1
            });
            await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
        }
        
        var current = await _dbContext.ApiUsages
            .AsNoTracking()
            .Where(u => u.Vendor == vendor && u.Date == dateKey)
            .Select(u => u.CallsMade)
            .FirstAsync(ct)
            .ConfigureAwait(false);

        _dailyCache[cacheKey] = (dateKey, current);

        await GetCallsLastMinuteAsync(vendor);
    }

    private Task<int> GetCallsLastMinuteAsync(string vendor)
    {
        var queue = _minuteWindows.GetOrAdd(vendor, _ => new ConcurrentQueue<DateTime>());
        CleanupOldEntries(queue, TimeSpan.FromMinutes(1));
        return Task.FromResult(queue.Count);
    }

    private static void CleanupOldEntries(ConcurrentQueue<DateTime> queue, TimeSpan window)
    {
        var cutoff = DateTime.UtcNow - window;
        while (queue.TryPeek(out var timestamp) && timestamp < cutoff)
            queue.TryDequeue(out _);
    }
}
