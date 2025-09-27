using Budget.Application.Interfaces;
using Currency.grpc;

namespace Budget.Infrastructure.ExternalServices
{
    public class GrpcCurrencyConverter(CurrencyRatesProtoService.CurrencyRatesProtoServiceClient grpcClient) : ICurrencyConverter
    {
        private readonly CurrencyRatesProtoService.CurrencyRatesProtoServiceClient _grpcClient = grpcClient;

        public async Task<decimal> ConvertAsync(string fromCurrency, string toCurrency, decimal amount)
        {
            GetConversionRequest request = new()
            {
                From = fromCurrency,
                To = toCurrency,
                Amount = (double) amount
            };

            GetConversionResponse response = await this._grpcClient.GetConversionAsync(request);

            return (decimal) response.Converted;
        }
    }
}
