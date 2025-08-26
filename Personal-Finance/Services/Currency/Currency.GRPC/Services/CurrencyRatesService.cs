using Currency.Common.Helpers;
using Currency.Common.Repositories;
using Currency.GRPC.Protos;
using Grpc.Core;

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

            var response = new GetSpecificCurrencyRatesResponse();
            foreach (var rate in rates.Rates)
            {
                var protoRate = new GetSpecificCurrencyRatesResponse.Types.CurrencyRate
                {
                    Code = rate.Code,
                    Date = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(
                        rate.Date.ToDateTime(TimeOnly.MinValue).ToUniversalTime()
                    ),
                    Parity = rate.Parity,
                    ExchangeMiddle = (double)rate.ExchangeMiddle
                };

                response.Rates.Add(protoRate);
            }

            return response;
        }
    }
}
