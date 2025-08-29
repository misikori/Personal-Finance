using Currency.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Currency.Common.DTOs
{
    public class CurrencyRateResponseDTO
    {
        public List<CurrencyRate> Rates { get; set; }
    }
}
