using Currency.grpc;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Portfolio.Core.Services;

/// <summary>
/// Implementation of currency converter using Currency gRPC service
/// </summary>
public class CurrencyConverterService : ICurrencyConverter
{
    private readonly CurrencyRatesProtoService.CurrencyRatesProtoServiceClient _grpcClient;
    private readonly ILogger<CurrencyConverterService> _logger;

    public CurrencyConverterService(
        CurrencyRatesProtoService.CurrencyRatesProtoServiceClient grpcClient,
        ILogger<CurrencyConverterService> logger)
    {
        _grpcClient = grpcClient ?? throw new ArgumentNullException(nameof(grpcClient));
        _logger = logger;
    }

    /// <summary>
    /// Converts amount from one currency to another using Currency service
    /// </summary>
    public async Task<decimal> ConvertAsync(string fromCurrency, string toCurrency, decimal amount)
    {
        // Skip conversion if same currency
        if (fromCurrency == toCurrency)
        {
            return amount;
        }

        try
        {
            var request = new GetConversionRequest
            {
                From = fromCurrency,
                To = toCurrency,
                Amount = (double)amount
            };

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var response = await _grpcClient.GetConversionAsync(request, cancellationToken: cts.Token);

            _logger.LogDebug(
                "Currency conversion: {Amount} {From} = {Converted} {To} (rate: {Rate})",
                amount, fromCurrency, response.Converted, toCurrency, response.Rate);

            return (decimal)response.Converted;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex,
                "gRPC error converting {From} to {To}. Status: {Status}",
                fromCurrency, toCurrency, ex.StatusCode);
            throw new InvalidOperationException($"Currency service unavailable: {ex.Status.Detail}", ex);
        }
    }
}

