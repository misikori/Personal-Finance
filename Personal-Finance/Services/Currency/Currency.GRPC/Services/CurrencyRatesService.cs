using Currency.Common.Entities;
using Currency.Common.Helpers;
using Currency.Common.Repositories;
using Currency.GRPC.Protos;
using Grpc.Core;
using System.Runtime.InteropServices;

namespace Currency.GRPC.Services
{
    public class CurrencyRatesService : CurrencyRatesProtoService.CurrencyRatesProtoServiceBase
    {
        private readonly ICurrencyRatesRepository _repository;
        private readonly ILogger<CurrencyRatesService> _logger;

        public CurrencyRatesService(ICurrencyRatesRepository repository, ILogger<CurrencyRatesService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async override Task<GetConversionResponse> GetConversion(GetConversionRequest request, ServerCallContext context)
        {
            var rates = await _repository.GetRates();
            var fromCurrency = rates.Rates.FirstOrDefault(x => x.Code == request.From);
            var toCurrency = rates.Rates.FirstOrDefault(y => y.Code == request.To);

            if (fromCurrency == null && toCurrency == null) {
                _logger.LogError($"Conversion from {request.From} to {request.To} unavailable.");
                throw new RpcException(new Status(StatusCode.NotFound, $"Conversion from {request.From} to {request.To} unavailable."));
            }
            _logger.LogInformation($"Converting {request.Amount} from {request.From} to {request.To}.");

            var converted = (double)CurrencyConverter.Convert((decimal)request.Amount, fromCurrency, toCurrency);

            return new GetConversionResponse
            {
                From = request.From,
                To = request.To,
                Amount = request.Amount,
                Rate = converted / request.Amount,
                Converted = converted
            };
        }

        public async override Task<GetSpecificCurrencyRatesResponse> GetSpecificCurrencyRates(GetSpecificCurrencyRatesRequest request, ServerCallContext context)
        {
            //return base.GetSpecificCurrencyRates(request, context);
            var rates = await _repository.GetRates();
            var baseCurrencyCode = string.IsNullOrWhiteSpace(request.BaseCurrency) ? "RSD" : request.BaseCurrency;

            double baseCurrencyRate = 0;
            foreach (var rate in rates.Rates) {
                if ( rate.Code == baseCurrencyCode)
                {
                    baseCurrencyRate = (double)rate.ExchangeMiddle;
                    break;
                }
            }

            if ( baseCurrencyRate == 0)
            {
                _logger.LogError($"Cannot recognize currency with code {baseCurrencyCode}.");
                throw new RpcException(new Status(StatusCode.NotFound, $"Error cannot recognize currency with code {baseCurrencyCode}."));
            }

            var response = new GetSpecificCurrencyRatesResponse();
            _logger.LogInformation($"Returning list of currency rates for base currency: {baseCurrencyCode}.");
            foreach (var rate in rates.Rates)
            {
                var protoRate = new GetSpecificCurrencyRatesResponse.Types.CurrencyRate
                {
                    Code = rate.Code,
                    Date = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(
                        rate.Date.ToDateTime(TimeOnly.MinValue).ToUniversalTime()
                    ),
                    Parity = rate.Parity,
                    ExchangeMiddle = (double)rate.ExchangeMiddle / baseCurrencyRate
                };

                response.Rates.Add(protoRate);
            }

            return response;
        }
    }
}
