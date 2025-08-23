using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Currency.Common.Entities
{
    public class CurrencyRate
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("date")]
        public DateOnly Date { get; set; }

        [JsonProperty("parity")]
        public int Parity { get; set; }
        
        [JsonProperty("exchange_middle")]
        public decimal ExchangeMiddle { get; set; }
    }
}
