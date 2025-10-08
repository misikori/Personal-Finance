using Currency.Common.Entities;
using Currency.Common.Helpers;
using Currency.Common.Repositories;
using Currency.GRPC.Protos;
using Grpc.Core;

namespace Currency.GRPC.Services
{
    public class CurrencyRatesService(ICurrencyRatesRepository repository, ILogger<CurrencyRatesService> logger) : CurrencyRatesProtoService.CurrencyRatesProtoServiceBase
    {
        private readonly ICurrencyRatesRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly ILogger<CurrencyRatesService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public override async Task<GetConversionResponse> GetConversion(GetConversionRequest request, ServerCallContext context)
        {
            Common.DTOs.CurrencyRateListDTO rates = await this._repository.GetRates();
            CurrencyRate? fromCurrency = rates.Rates.FirstOrDefault(x => x.Code == request.From);
            CurrencyRate? toCurrency = rates.Rates.FirstOrDefault(y => y.Code == request.To);

            if (fromCurrency == null || toCurrency == null)
            {
                this._logger.LogError($"Conversion from {request.From} to {request.To} unavailable.");
                throw new RpcException(new Status(StatusCode.NotFound, $"Conversion from {request.From} to {request.To} unavailable."));
            }
            this._logger.LogInformation($"Converting {request.Amount} from {request.From} to {request.To}.");

            double converted = (double) CurrencyConverter.Convert((decimal) request.Amount, fromCurrency, toCurrency);

            return new GetConversionResponse
            {
                From = request.From,
                To = request.To,
                Amount = request.Amount,
                Rate = converted / request.Amount,
                Converted = converted
            };
        }

        public override async Task<GetSpecificCurrencyRatesResponse> GetSpecificCurrencyRates(GetSpecificCurrencyRatesRequest request, ServerCallContext context)
        {
            Common.DTOs.CurrencyRateListDTO rates = await this._repository.GetRates();
            string baseCurrencyCode = string.IsNullOrWhiteSpace(request.BaseCurrency) ? "RSD" : request.BaseCurrency;

            double baseCurrencyRate = 0;
            foreach (CurrencyRate rate in rates.Rates)
            {
                if (rate.Code == baseCurrencyCode)
                {
                    baseCurrencyRate = (double) rate.ExchangeMiddle;
                    break;
                }
            }

            if (baseCurrencyRate == 0)
            {
                this._logger.LogError($"Cannot recognize currency with code {baseCurrencyCode}.");
                throw new RpcException(new Status(StatusCode.NotFound, $"Error cannot recognize currency with code {baseCurrencyCode}."));
            }

            GetSpecificCurrencyRatesResponse response = new();
            this._logger.LogInformation($"Returning list of currency rates for base currency: {baseCurrencyCode}.");
            foreach (CurrencyRate rate in rates.Rates)
            {
                GetSpecificCurrencyRatesResponse.Types.CurrencyRate protoRate = new()
                {
                    Code = rate.Code,
                    Date = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(
                        rate.Date.ToDateTime(TimeOnly.MinValue).ToUniversalTime()
                    ),
                    Parity = rate.Parity,
                    ExchangeMiddle = (double) rate.ExchangeMiddle / baseCurrencyRate
                };

                response.Rates.Add(protoRate);
            }

            return response;
        }
    }
}
