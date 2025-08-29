using Currency.Common.DTOs;
using Currency.Common.Entities;
using Currency.Common.Helpers;
using Currency.Common.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Currency.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CurrencyController : ControllerBase
    {
        private readonly ICurrencyRatesRepository _repository;

        public CurrencyController(ICurrencyRatesRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<CurrencyRate>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<CurrencyRate>>> GetRates([FromQuery] string baseCurrencyCode = "RSD")
        {
            var rates = await _repository.GetRates();

            if (rates == null)
            {
                return NotFound();
            }

            decimal baseCurrencyRate = 0;

            if (baseCurrencyCode != "RSD")
            {
                foreach (var rate in rates.Rates)
                {
                    if (rate.Code == baseCurrencyCode)
                    {
                        baseCurrencyRate = rate.ExchangeMiddle;
                    }
                }

                if ( baseCurrencyRate == 0)
                {
                    return BadRequest();
                }

                foreach (var rate in rates.Rates)
                {
                    rate.ExchangeMiddle /= baseCurrencyRate;
                }
            }
            return Ok(rates);
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
            var rates = await _repository.GetRates();
            if (rates == null)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable);
            }

            var fromCurrency = rates.Rates.FirstOrDefault(x => x.Code == from);
            var toCurrency = rates.Rates.FirstOrDefault(x => x.Code == to);

            if (fromCurrency == null || toCurrency == null) {
                return NotFound($"Conversion from {from} to {to} not found!");
            }

            decimal converted = CurrencyConverter.Convert(amount, fromCurrency, toCurrency);

            return Ok(new CurrencyConverterResponseDTO
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
        public async Task<ActionResult<CurrencyRateListDTO>> UpdateRates([FromBody]CurrencyRateListDTO rates)
        {
            return Ok(await _repository.UpdateRates(rates));
        }

        [HttpDelete]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteRates()
        {
            await _repository.DeleteRates();
            return Ok();
        }
    }
}
