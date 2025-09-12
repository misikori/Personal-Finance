using System.Text.RegularExpressions;
using MarketGateway.Data.Interfaces;
using MarketGateway.Providers.Configuration;
using MarketGateway.Providers.Interfaces;
using MarketGateway.Shared.API;
using MarketGateway.Shared.DTOs;
using Microsoft.Extensions.Logging;

namespace MarketGateway.Providers.Providers;

using APIResult = ApiResult<MarketDataResultBase>;

public partial class  VendorMarketDataProvider:IMarketDataProvider
{
    public string VendorName => Config.Vendor;
    public VendorConfig Config { get; }
    
    private readonly IApiUsageTracker _usageTracker;
    private readonly HttpClient _httpClient;
    private readonly IVendorResponseParser _parser;
    private readonly ILogger<VendorMarketDataProvider> _log;

    [GeneratedRegex("{(?<t>[^}]+)}", RegexOptions.CultureInvariant)]
    private static partial Regex TokenRegex();

    public VendorMarketDataProvider(
        VendorConfig config,
        IHttpClientFactory httpClientFactory,
        IApiUsageTracker usageTracker,
        IVendorResponseParser parser,
        ILogger<VendorMarketDataProvider> log)
    {
        Config         = config;
        _httpClient    = httpClientFactory.CreateClient($"vendor:{config.Vendor}");
        _usageTracker  = usageTracker;
        _parser        = parser;
        _log           = log;
    }

    #region IMarketDataProvider
    public async Task<FetchGate> CanFetchDataAsync(MarketDataRequest request, CancellationToken token)
    {
        if (Config.Endpoints.Values.All(e => e.DataType != request.Type))
            return new FetchGate(false, "Endpoint for requested type is not supported", null);

        var todayUtc = DateTime.UtcNow.Date;
        var perDay   = await _usageTracker.GetCallsMadeAsync(VendorName, todayUtc, token);

        if (Config.RateLimits.PerDay > 0 && perDay >= Config.RateLimits.PerDay)
            return new FetchGate(false, "Daily rate limit reached", todayUtc.AddDays(1));

        // TODO: if/when IApiUsageTracker exposes a rolling window method,
        // enforce PerMinute here (e.g., GetCallsInWindowAsync(window: 1 minute)).

        return new FetchGate(true, null, null);
    }

    
    
    public async Task<APIResult> FetchAsync(MarketDataRequest request, CancellationToken token)
    {
        
        var endpoint = Config.Endpoints.Values.FirstOrDefault(e => e.DataType == request.Type);
        if (endpoint == null)
            return APIResult.Fail("Endpoint for requested type is not supported");
        
        // TODO: This should be fixed or done smarter for different API calls
        var reqId = request.Ids.FirstOrDefault();
        var identifier = reqId?.Symbol ?? "(unknown)";
        
        var gate = await CanFetchDataAsync(request, token);
        if (!gate.Allowed) return APIResult.Fail(gate.Reason ?? "Rate limit reached", retryAfter: gate.RetryAfter);

        
        try
        {
            var url = BuildUrl(endpoint, request);
            using var req = new HttpRequestMessage(new HttpMethod(endpoint.HttpMethod), url);

            
            using var response = await _httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, token);
            var body = await response.Content.ReadAsStringAsync(token);
            _log.LogWarning("This is body: {body}",body);
            if (!response.IsSuccessStatusCode)
            {
                return APIResult.Fail(
                    error: $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}. Body: {body}",
                    status: response.StatusCode
                );
            }
            
            // TODO: Parsing should be done smarter
            var parsed = _parser.Parse(Config, request, body);
            parsed.Vendor = VendorName;           
            parsed.Type   = endpoint.DataType;
            parsed.Id ??=  new IdentifierDto(reqId?.Symbol ?? string.Empty);
            parsed.Id ??= reqId;
            _log.LogWarning("This is parsed: {parsed}",parsed);
            
            
            try { await _usageTracker.IncrementUsageAsync(VendorName, DateTime.UtcNow, token); }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Failed to increment usage for {Vendor}", VendorName);
            }

            return APIResult.Ok(parsed, response.StatusCode);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            _log.LogWarning("Request cancelled for {Vendor}/{Identifier}", VendorName, identifier);
            return APIResult.Fail("Request timed out/cancelled.");
        }
        catch (InvalidOperationException ex)
        {
            _log.LogWarning(ex, "Invalid request for {Vendor}/{Identifier}", VendorName, identifier);
            return APIResult.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Request failed for {Vendor}/{Identifier}", VendorName, identifier);
            return APIResult.Fail($"Request failed: {ex.Message}");
        }
    }
    #endregion
    
    
    #region Private Methods
    private string BuildUrl(EndpointConfig endpoint, MarketDataRequest request)
    {
        var baseUri = new Uri(new Uri(Config.BaseUrl, UriKind.Absolute), endpoint.Path ?? string.Empty);
        var allParams = new List<KeyValuePair<string, string>>();

        if (!string.IsNullOrWhiteSpace(endpoint.Function))
            allParams.Add(new KeyValuePair<string, string>("function", endpoint.Function));
        
        foreach (var kv in endpoint.QueryParams.Required)
        {
            var resolved = ResolveTemplate(kv.Value, request).Trim();
            if (string.IsNullOrEmpty(resolved))
                throw new InvalidOperationException($"Required query parameter '{kv.Key}' resolved empty for vendor '{VendorName}'.");
            allParams.Add(new KeyValuePair<string, string>(kv.Key, resolved));
        }
        
        foreach (var kv in endpoint.QueryParams.Optional)
        {
            var resolved = ResolveTemplate(kv.Value, request).Trim();
            if (!string.IsNullOrWhiteSpace(resolved))
                allParams.Add(new KeyValuePair<string, string>(kv.Key, resolved));
        }
        
        if (!string.IsNullOrWhiteSpace(Config.ApiKey) &&
            !allParams.Any(p => p.Key.Equals("apikey", StringComparison.OrdinalIgnoreCase)))
        {
            allParams.Add(new KeyValuePair<string, string>("apikey", Config.ApiKey));
        }

        var query = string.Join("&",
            allParams.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));

        return query.Length == 0 ? baseUri.ToString() : $"{baseUri}?{query}";
    }
    
    private static string? TryGetSymbol(MarketDataRequest r)
    {
        if (r.Ids.Count > 0 && !string.IsNullOrWhiteSpace(r.Ids[0].Symbol))
            return r.Ids[0].Symbol;
        return null;
    }
    
    private string ResolveTemplate(string raw, MarketDataRequest req)
    {
        if (string.IsNullOrEmpty(raw)) return raw;

        var symbol   = TryGetSymbol(req); 
        var datatype = req.Parameters.TryGetValue("datatype", out var dtObj) ? dtObj?.ToString() : null;
        var date     = req.Parameters.TryGetValue("date", out var dObj) ? dObj?.ToString() : null;
        
        string? interval = null;
        if (req.Parameters.TryGetValue("interval", out var iObj)) interval = iObj?.ToString();
        else if (req.Parameters.TryGetValue("granularity", out var gObj)) interval = gObj?.ToString();

        var map = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["symbol"]     = symbol,
            ["identifier"] = symbol,       
            ["datatype"]   = datatype,
            ["date"]       = date,
            ["interval"]   = interval,
            ["apikey"]     = Config.ApiKey,
            ["vendor"]     = Config.Vendor
        };

        return TokenRegex().Replace(raw, m =>
        {
            var key = m.Groups["t"].Value;
            return (map.TryGetValue(key, out var v) && !string.IsNullOrEmpty(v)) ? v : "";
        });
    }
    
    

    #endregion

}