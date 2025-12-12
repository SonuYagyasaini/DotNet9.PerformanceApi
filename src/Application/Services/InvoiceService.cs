using Application.Interfaces;
using Azure.Messaging.ServiceBus;
using Domain.Dtos;
using System.Text.Json;
using Microsoft.Extensions.Configuration;


namespace Application.Services;

public class InvoiceService : IInvoiceService
{
    private readonly ServiceBusClient _sbClient;
    private readonly IConfiguration _config;

    public InvoiceService(ServiceBusClient sbClient, IConfiguration config)
    {
        _sbClient = sbClient;
        _config = config;
    }

    public async Task<string> EnqueueInvoicesAsync(BulkInvoiceRequest req)
    {
        var sender = _sbClient.CreateSender(_config.GetValue<string>("ServiceBus:QueueName") ?? "invoices-queue");

        ServiceBusMessageBatch mb = await sender.CreateMessageBatchAsync();

        int count = 0;

        foreach (var dto in req.Data)
        {
            var msg = new ServiceBusMessage(JsonSerializer.Serialize(dto))
            {
                MessageId = Guid.NewGuid().ToString()
            };

            if (!mb.TryAddMessage(msg))
            {
                // Send full batch
                await sender.SendMessagesAsync(mb);
                mb.Dispose();

                // Create new batch
                mb = await sender.CreateMessageBatchAsync();
                mb.TryAddMessage(msg);
            }

            count++;
        }

        if (mb.Count > 0)
        {
            await sender.SendMessagesAsync(mb);
            mb.Dispose();
        }

        return $"{count} messages enqueued.";
    }

}
