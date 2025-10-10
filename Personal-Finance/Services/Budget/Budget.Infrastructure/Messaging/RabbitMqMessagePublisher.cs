using Budget.Application.Interfaces;
using EventBus.Messages.Constants;
using EventBus.Messages.Events;
using MassTransit;

namespace Budget.Infrastructure.Messaging;

public class RabbitMqMessagePublisher : IMessagePublisher
{
    private readonly ISendEndpointProvider _sendEndpointProvider;

    public RabbitMqMessagePublisher(ISendEndpointProvider sendEndpointProvider)
    {
        this._sendEndpointProvider = sendEndpointProvider;
    }

    public async Task SendTransactionsReport(TransactionsReportEvent message)
    {
        var endpoint = await this._sendEndpointProvider.GetSendEndpoint(
            new Uri($"queue:{EventBusConstants.TansactionsReportQueue}")
            );

        await endpoint.Send(message);
    }
}
