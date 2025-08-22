using Currency.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Currency.Common.DTOs
{
    public class CurrencyRateListDTO
    {
        public string Key { get; set; }
        public List<CurrencyRate> Rates { get; set; } = new List<CurrencyRate>();
    }
}
