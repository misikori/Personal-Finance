using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Messages.Events
{
    public enum TransactionType
    {
        Income,
        Expense
    }

    public class TransactionItem
    {
        public decimal Amount { get; set; }
        public string WalletName { get; set; }
        public TransactionType TransactionType { get; set; }
        public string Description { get; set; }
        public DateTime Date {  get; set; }
        public required string Currency { get; set; }
        public required string CategoryName { get; set; }
    }

    public class TransactionsReportEvent
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public IEnumerable<TransactionItem> TransactionItems { get; set; }
    }
}
