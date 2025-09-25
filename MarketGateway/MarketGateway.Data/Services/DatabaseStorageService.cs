using MarketGateway.Data.Interfaces;
using MarketGateway.Data.Services.Ohlcv;
using MarketGateway.Data.Services.Quotes;
using MarketGateway.Shared.DTOs;
using Serilog;

namespace MarketGateway.Data.Services;

public sealed class DatabaseStorageService : IStorageService
{
    private readonly IQuoteStorage _quotes;
    private readonly IOhlcvStorage _ohlcv;

    public DatabaseStorageService(IQuoteStorage quotes, IOhlcvStorage ohlcv)
    {
        _quotes = quotes;
        _ohlcv  = ohlcv;
    }

    public async Task<MarketDataResultBase?> TryReadAsync(MarketDataRequest request, CancellationToken ct = default)
    {
        Log.Information("Storage.TryReadAsync type={Type} ids={Ids} params={HasParams}",
            request.Type,
            string.Join(",", request.Ids?.Select(i => i.Symbol) ?? Array.Empty<string>()),
            request.Parameters is { Count: > 0 });

        MarketDataResultBase? result = request.Type switch
        {
            DataType.Quote  => await _quotes.TryReadAsync(request, ct),
            DataType.OHLCV  => await _ohlcv.TryReadAsync(request, ct),
            _               => null
        };

        Log.Information("Storage.TryReadAsync result={HitOrMiss}",
            result is null ? "MISS" : "HIT");
        return result;
    }

    public Task SaveParsedResultAsync(MarketDataResultBase result, CancellationToken ct = default)
        => result switch
        {
            QuoteDto q        => _quotes.SaveAsync(q, ct),
            OhlcvSeriesDto s  => _ohlcv.SaveAsync(s, ct),
            _ => Task.FromException(new NotSupportedException(
                $"No storage mapping for {result.GetType().Name} ({result.Type})"))
        };

    public Task RecordParseFailureAsync(string vendor, DataType type, string identifier, string error,
        DateTime? occurredUtc = null, CancellationToken ct = default)
        => _ohlcv.RecordParseFailureAsync(vendor, type, identifier, error, occurredUtc, ct); 
}