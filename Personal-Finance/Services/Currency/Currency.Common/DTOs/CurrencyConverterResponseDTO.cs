namespace Currency.Common.DTOs
{
    public class CurrencyConverterResponseDTO
    {
        public string From { get; set; }
        public string To { get; set; }
        public decimal Amount { get; set; }
        public decimal Rate { get; set; }
        public decimal Converted { get; set; }
    }
}
