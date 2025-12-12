using Azure.Messaging.ServiceBus;
using Domain.Dtos;
using Infrastructure.Bulk;
using Infrastructure.Db;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text.Json; 
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;


public class InvoiceQueueWorker : BackgroundService
{
    private readonly ServiceBusClient _client;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _config;
    private const int BatchSize = 5000;

    public InvoiceQueueWorker(ServiceBusClient client, IServiceScopeFactory scopeFactory, IConfiguration config)
    {
        _client = client;
        _scopeFactory = scopeFactory;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queueName = _config.GetValue<string>("ServiceBus:QueueName") ?? "invoices-queue";
        var processor = _client.CreateProcessor(queueName, new ServiceBusProcessorOptions { MaxConcurrentCalls = 4, AutoCompleteMessages = false });

        var buffer = new List<Domain.Dtos.InvoiceDto>();

        processor.ProcessMessageAsync += async args =>
        {
            try
            {
                var dto = JsonSerializer.Deserialize<Domain.Dtos.InvoiceDto>(args.Message.Body.ToString());
                if (dto != null)
                {
                    lock (buffer) buffer.Add(dto);
                }

                if (buffer.Count >= BatchSize)
                {
                    List<Domain.Dtos.InvoiceDto> toProcess;
                    lock (buffer)
                    {
                        toProcess = buffer.Take(BatchSize).ToList();
                        buffer.RemoveRange(0, toProcess.Count);
                    }
                    await ProcessBatchAsync(toProcess);
                }

                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await args.AbandonMessageAsync(args.Message);
            }
        };

        processor.ProcessErrorAsync += args =>
        {
            Console.WriteLine(args.Exception);
            return Task.CompletedTask;
        };

        await processor.StartProcessingAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(2000, stoppingToken);
            List<Domain.Dtos.InvoiceDto> toFlush = null;
            lock (buffer)
            {
                if (buffer.Count > 0)
                {
                    toFlush = buffer.ToList();
                    buffer.Clear();
                }
            }
            if (toFlush != null && toFlush.Count > 0)
            {
                await ProcessBatchAsync(toFlush);
            }
        }

        await processor.StopProcessingAsync();
    }

    private async Task ProcessBatchAsync(List<Domain.Dtos.InvoiceDto> batch)
    {
        var dt = new DataTable();
        dt.Columns.Add("Customer", typeof(string));
        dt.Columns.Add("Amount", typeof(decimal));
        dt.Columns.Add("CreatedOn", typeof(DateTime));
        foreach (var b in batch)
            dt.Rows.Add(b.Customer, b.Amount, DateTime.UtcNow);

        var conn = _config.GetConnectionString("Db");
        var helper = new Infrastructure.Bulk.BulkInsertHelper(conn);
        await helper.WriteAsync(dt, "Invoices");
    }
}
