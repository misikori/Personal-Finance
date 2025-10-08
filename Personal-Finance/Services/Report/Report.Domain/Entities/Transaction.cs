using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report.Domain.Entities
{
    public sealed class Transaction
    {
        public decimal Amount { get; }
        public string WalletName { get; }
        public string Description { get; }
        public DateTime Date { get; }
        public string Currency { get; }
        public string CategoryName { get; }
        public bool IsIncome { get; }

        public Transaction(decimal amount, string walletName, string description, DateTime date,
                           string currency, string categoryName, bool isIncome)
        {
            Amount = amount;
            WalletName = walletName;
            Description = description;
            Date = date;
            Currency = currency;
            CategoryName = categoryName;
            IsIncome = isIncome;
        }
    }
}
