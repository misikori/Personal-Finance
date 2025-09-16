using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Budget.Domain.Entities
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public TransactionType TransactionType { get; set; }
        public string Description { get; set; }
        public DateTime Date {  get; set; }
        public string Currency { get; set; }

        // foreign key to Wallet
        public Guid WalletId { get; set; }
        public Wallet Wallet { get; set; }


    }
}
