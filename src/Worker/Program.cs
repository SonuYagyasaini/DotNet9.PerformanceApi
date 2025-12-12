using Azure.Messaging.ServiceBus;
using Infrastructure.Db;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureServices((ctx, services) =>
{
    services.AddDbContext<AppDbContext>(o => o.UseSqlServer(ctx.Configuration.GetConnectionString("Db")));
    var sbConn = ctx.Configuration.GetValue<string>("ServiceBus:Conn") ?? "";
    services.AddSingleton(new ServiceBusClient(sbConn));
    services.AddHostedService<InvoiceQueueWorker>();
});
var host = builder.Build();
await host.RunAsync();
