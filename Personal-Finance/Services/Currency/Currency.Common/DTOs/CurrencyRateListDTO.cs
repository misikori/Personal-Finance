using Currency.Common.Entities;

namespace Currency.Common.DTOs
{
    public class CurrencyRateListDTO
    {
        public string Key { get; set; }
        public List<CurrencyRate> Rates { get; set; } = [];
    }
}
