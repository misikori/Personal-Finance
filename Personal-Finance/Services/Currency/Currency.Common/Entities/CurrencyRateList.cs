using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Currency.Common.Entities
{
    public class CurrencyRateList
    {
        public string Username { get; set; }
        public List<CurrencyRate> Rates { get; set; } = new List<CurrencyRate>();
    }
}
