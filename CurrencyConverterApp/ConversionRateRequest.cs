namespace CurrencyConverterApp
{
    public class ConversionRateRequest
    {
        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
        public double Rate { get; set; }
    }
}
