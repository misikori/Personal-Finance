using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Currency.Common.Entities
{
    internal class CurrencyRate
    {
        public string Code { get; set; }
        public DateTime Date { get; set; }
        public decimal MiddleRate { get; set; }
    }
}
