using Currency.Common.DTOs;
using Currency.Common.Entities;
using Currency.Common.Helpers;
using Currency.Common.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Currency.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CurrencyController(ICurrencyRatesRepository repository) : ControllerBase
    {
        private readonly ICurrencyRatesRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

        [HttpGet]
        [ProducesResponseType(typeof(List<CurrencyRate>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<CurrencyRate>>> GetRates([FromQuery] string baseCurrencyCode = "RSD")
        {
            CurrencyRateListDTO rates = await this._repository.GetRates();

            if (rates.Rates.Count == 0)
            {
                return this.NotFound();
            }

            if (baseCurrencyCode != "RSD")
            {
                decimal baseCurrencyRate = rates.Rates.FirstOrDefault(r => r.Code == baseCurrencyCode)?.ExchangeMiddle ?? 0;

                if (baseCurrencyRate == 0)
                {
                    return this.BadRequest();
                }

                rates.Rates.ForEach(r => r.ExchangeMiddle /= baseCurrencyRate);
            }
            return this.Ok(rates);
        }

        [HttpGet("convert")]
        [ProducesResponseType(typeof(CurrencyConverterResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(void), StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<CurrencyConverterResponseDTO>> GetConverted(
            [FromQuery] string from,
            [FromQuery] string to,
            [FromQuery] decimal amount)
        {
            CurrencyRateListDTO rates = await this._repository.GetRates();
            if (rates.Rates.Count == 0)
            {
                return this.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }

            CurrencyRate? fromCurrency = rates.Rates.FirstOrDefault(x => x.Code == from);
            CurrencyRate? toCurrency = rates.Rates.FirstOrDefault(x => x.Code == to);

            if (fromCurrency == null || toCurrency == null)
            {
                return this.NotFound($"Conversion from {from} to {to} not found!");
            }

            decimal converted = CurrencyConverter.Convert(amount, fromCurrency, toCurrency);

            return this.Ok(new CurrencyConverterResponseDTO
            {
                From = fromCurrency.Code,
                To = toCurrency.Code,
                Amount = amount,
                Rate = converted / amount,
                Converted = converted
            });

        }

        [HttpPut]
        [ProducesResponseType(typeof(CurrencyRateListDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult<CurrencyRateListDTO>> UpdateRates([FromBody] CurrencyRateListDTO rates) => this.Ok(await this._repository.UpdateRates(rates));

        [HttpDelete]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteRates()
        {
            await this._repository.DeleteRates();
            return this.Ok();
        }
    }
}
