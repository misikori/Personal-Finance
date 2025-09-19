using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Budget.Domain.Entities
{
    public class Wallet
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public required string Name { get; set; }
        public required string Currency { get; set; }
        public decimal CurrentBalance { get; set; }
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    }
}
