using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverterApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CurrencyConverterController: ControllerBase
    {
        private readonly ICurrencyConverter _currencyConverter;

        public CurrencyConverterController(ICurrencyConverter currencyConverter)
        {
            _currencyConverter = currencyConverter;
        }

        [HttpPost("configure")]
        public IActionResult Configure(List<ConversionRateRequest> rates)
        {
            _currencyConverter.ClearConfiguration();
            _currencyConverter.UpdateConfiguration(rates.Select(r => new Tuple<string, string, double>(r.FromCurrency.ToUpper(), r.ToCurrency.ToUpper(), r.Rate)));
            return Ok();
        }

        [HttpGet("convert")]
        public IActionResult Convert(string fromCurrency, string toCurrency, double amount)
        {
            try
            {
                double result = _currencyConverter.Convert(fromCurrency.ToUpper(), toCurrency.ToUpper(), amount);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
