using Currency.Common.Entities;
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
        public async Task<ActionResult<List<CurrencyRate>>> GetRates()
        {
            //username should not be in query but just for testing for now it will be implemented like this
            //username should be provided from Auth (jwt...)
            var rates = await _repository.GetRates("TEST");

            //if (string.IsNullOrEmpty(username))
            //    return Unauthorized();

            if (rates == null)
            {
                return NotFound();
            }

            return Ok(rates.Rates);
        }

        [HttpPut]
        [ProducesResponseType(typeof(CurrencyRateList), StatusCodes.Status200OK)]
        public async Task<ActionResult<CurrencyRateList>> UpdateRates([FromBody]CurrencyRateList rates)
        {
            return Ok(await _repository.UpdateRates(rates));
        }

        [HttpDelete("{username}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteRates(string username)
        {
            await _repository.DeleteRates(username);
            return Ok();
        }
    }
}
