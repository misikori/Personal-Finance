using MarketGateway.Data.Interfaces;
using MarketGateway.Interfaces;
using MarketGateway.Providers.Interfaces;
using MarketGateway.Shared.API;
using MarketGateway.Shared.DTOs;

namespace MarketGateway.Application;

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


    public async Task<ApiResult<MarketDataResultBase>> FetchAsync(MarketDataRequest request, CancellationToken ct)
    {
        var candidates = CandidatesByType(request).ToList();
        if (!candidates.Any())
            return ApiResult<MarketDataResultBase>.Fail($"No providers support {request.Type}");

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

                    return ApiResult<MarketDataResultBase>.Ok(parsed);
                }

                _log.LogDebug("Storage hit but stale for {Vendor}/{Type}/{Identifier}",
                    p.VendorName, request.Type, identifier);
            }
            catch (Exception ex)
            {
                _log.LogInformation(ex, "Stored JSON parse failed for {Vendor}/{Identifier}", p.VendorName, identifier);
            }
        }
        
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
                _log.LogInformation("It's not allowed");
                continue;
            }
            _log.LogInformation("Fetching data for {Vendor}/{Type}/{Identifier}", p.VendorName, request.Type, identifier);
            var res = await p.FetchAsync(request, ct);
            if (res.Success && res.Data is not null)
            {
                try
                {
                  //  await _storage.SaveApiResponseAsync(p.VendorName, identifier, res.ToString(), ct);
                    switch (res.Data)
                    {
                        case QuoteDto q:
                            await _storage.SaveParsedResultAsync(q);
                            return ApiResult<MarketDataResultBase>.Ok(q);
                            break;
                        // TODO: add other DTO types here as you support them
                    }

                    break; // I should hit only one vendor ?
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
        return ApiResult<MarketDataResultBase>.Fail(msg, retryAfter: bestRetry);
    }

    public IAsyncEnumerable<ApiResult<MarketDataResultBase>> FetchStreamAsync(MarketDataRequest request, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    private IEnumerable<IMarketDataProvider> CandidatesByType(MarketDataRequest req)
    {
        var all = _resolver.GetAll().Values.ToList();
        _log.LogInformation("All candidates = {all}",all);
        _log.LogInformation("Candidate discovery for type={Type}. All providers: {Providers}",
            req.Type,
            string.Join(", ", all.Select(p =>
                $"{p.VendorName}[{string.Join("|", p.Config.Endpoints.Values.Select(e => e.DataType).Distinct())}]")));
        var supporting = all.Where(p => p.Config.Supports(req.Type)).ToList();

        if (supporting.Count == 0)
        {
            _log.LogWarning("No providers support type {Type}. Check YAML endpoint 'dataType' values and enum mapping.", req.Type);
            return Enumerable.Empty<IMarketDataProvider>();
        }
        
        if (req.PreferredVendors is { Count: > 0 })
        {
            var pref = new HashSet<string>(req.PreferredVendors, StringComparer.OrdinalIgnoreCase);

            var ordered = supporting
                .OrderBy(p => pref.Contains(p.VendorName) ? 0 : 1)
                .ThenBy(p => p.VendorName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var unknownPrefs = pref.Except(supporting.Select(p => p.VendorName), StringComparer.OrdinalIgnoreCase).ToList();
            if (unknownPrefs.Count > 0)
                _log.LogInformation("Preferred vendors not available for type {Type}: {Missing}", req.Type, string.Join(", ", unknownPrefs));

            _log.LogInformation("Selected candidates (preferred first): {Vendors}",
                string.Join(", ", ordered.Select(p => p.VendorName)));

            return [ordered.First()];
        }

        _log.LogInformation("Selected candidates: {Vendors}",
            string.Join(", ", supporting.Select(p => p.VendorName)));

        return supporting;
    }

    private static bool IsFreshEnough(MarketDataRequest req, MarketDataResultBase parsed)
    {
        DateTimeOffset? ts = parsed switch
        {
            QuoteDto q => q.TimestampUtc,
            _ => null
        };
        if (!ts.HasValue) return false;
        
        var window = req.Type switch
        {
            DataType.Quote => TimeSpan.FromHours(24),
            _ => TimeSpan.FromDays(7)
        };
        
        var t = DateTime.SpecifyKind(ts.Value.DateTime, DateTimeKind.Utc);
        var now = DateTime.UtcNow;
        if (t > now) return true;

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
