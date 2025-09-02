using MarketGateway.Data.Interfaces;
using MarketGateway.Interfaces;
using MarketGateway.Shared.API;
using MarketGateway.Shared.DTOs;

namespace MarketGateway.Services;

public sealed class MarketDataBroker : IMarketDataBroker
{
    private readonly IMarketDataProviderResolver _resolver;
    private readonly IStorageService _storage;
    private readonly IVendorResponseParser _parser;
    private readonly ILogger<MarketDataBroker> _log;


    public MarketDataBroker(IMarketDataProviderResolver resolver,
        IStorageService storage,
        IVendorResponseParser parser,
        ILogger<MarketDataBroker> log)
    {
        _resolver = resolver;
        _storage  = storage;
        _parser   = parser;
        _log      = log;
    }


    public async Task<APIResult<MarketDataResultBase>> FetchAsync(MarketDataRequest request, CancellationToken ct)
    {
        var candidates = CandidatesByType(request).ToList();
        if (!candidates.Any())
            return APIResult<MarketDataResultBase>.Fail($"No providers support {request.Type}");

        var identifier = GetRequestIdentifier(request);

        foreach (var p in candidates)
        {
            var raw = await _storage.TryReadLatestAsync(p.VendorName, identifier, null, ct);
            if (string.IsNullOrWhiteSpace(raw)) continue;

            try
            {
                var parsed = _parser.Parse(p.Config, request, raw);
                if (IsFreshEnough(request, parsed))
                {
                    _log.LogDebug("Storage hit (fresh) for {Vendor}/{Type}/{Identifier}",
                        p.VendorName, request.Type, identifier);

                    return APIResult<MarketDataResultBase>.Ok(parsed);
                }

                _log.LogDebug("Storage hit but stale for {Vendor}/{Type}/{Identifier}",
                    p.VendorName, request.Type, identifier);
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Stored JSON parse failed for {Vendor}/{Identifier}", p.VendorName, identifier);
            }
        }

        // 2) LIVE FETCH if storage is missing/stale
        DateTimeOffset? bestRetry = null;
        var errors = new List<string>(capacity: candidates.Count);

        foreach (var p in candidates)
        {
            ct.ThrowIfCancellationRequested();

            var (allowed, reason, retry) = await p.CanFetchDataAsync(request, ct);
            if (!allowed)
            {
                bestRetry = Min(bestRetry, retry);
                errors.Add($"{p.VendorName}: {reason}");
                continue;
            }

            var res = await p.FetchAsync(request, ct);
            if (res.Success && res.Data is not null)
            {
                try
                {
                    await _storage.SaveApiResponseAsync(p.VendorName, identifier, res.Data.ToString(), ct);
                }
                catch (Exception ex)
                {
                    _log.LogWarning(ex, "Failed to persist raw payload for {Vendor}/{Identifier}", p.VendorName,
                        identifier);
                }

                try
                {
                    switch (res.Data)
                    {
                        case QuoteDto q:
                            await _storage.SaveParsedResultAsync(q);
                            break;
                        // TODO: add other DTO types here as you support them
                    }
                }
                catch (Exception ex)
                {
                    _log.LogWarning(ex, "Failed to persist parsed result for {Vendor}/{Identifier}", p.VendorName,
                        identifier);
                }
                return res;
            }

            errors.Add($"{p.VendorName}: {res.Error ?? "fetch failed"}");
        }

        var msg = errors.Count > 0 ? string.Join("; ", errors) : "All providers failed";
        return APIResult<MarketDataResultBase>.Fail(msg, retryAfter: bestRetry);
    }

    public IAsyncEnumerable<APIResult<MarketDataResultBase>> FetchStreamAsync(MarketDataRequest request, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    private IEnumerable<IMarketDataProvider> CandidatesByType(MarketDataRequest req)
    {
        var all = _resolver.GetAll().Values
            .Where(p => p.Config.Endpoints.Values.Any(e => e.DataType == req.Type));

        if (req.PreferredVendors is { Count: > 0 })
        {
            var pref = new HashSet<string>(req.PreferredVendors, StringComparer.OrdinalIgnoreCase);
            var preferred = all.Where(p => pref.Contains(p.VendorName));
            var others    = all.Where(p => !pref.Contains(p.VendorName));
            return preferred.Concat(others);
        }
        return all;
    }
    private static bool IsFreshEnough(MarketDataRequest req, MarketDataResultBase parsed)
    {
        DateTime? ts = parsed switch
        {
            QuoteDto q => q.Timestamp,
            _ => null
        };
        if (!ts.HasValue) return false;
        
        var window = req.Type switch
        {
            DataType.Quote => TimeSpan.FromHours(24),
            _ => TimeSpan.FromDays(7)
        };
        
        var t = DateTime.SpecifyKind(ts.Value, DateTimeKind.Utc);
        var now = DateTime.UtcNow;
        if (t > now) t = now;

        return (now - t) <= window;
    }
    private static string GetRequestIdentifier(MarketDataRequest r)
    {
        if (r?.Ids is { Count: > 0 } && !string.IsNullOrWhiteSpace(r.Ids[0].Symbol))
            return r.Ids[0].Symbol;

        if (r?.Parameters?.Count > 0)
        {
            var firstKey = r.Parameters.Keys.First();
            var firstVal = r.Parameters[firstKey]?.ToString();
            if (!string.IsNullOrWhiteSpace(firstVal))
                return $"{firstKey}:{firstVal}";
        }
        return $"unknown-{r?.Type}";
    }
    

    private static DateTimeOffset? Min(DateTimeOffset? a, DateTimeOffset? b)
        => (a, b) switch
        {
            (null, null) => null,
            (null, var x) => x,
            (var x, null) => x,
            (var x, var y) => x < y ? x : y
        };
}
