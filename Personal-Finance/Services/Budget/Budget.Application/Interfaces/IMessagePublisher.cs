using EventBus.Messages.Events;

namespace Budget.Application.Interfaces;

public interface IMessagePublisher
{
    Task SendTransactionsReport(TransactionsReportEvent message);
}
