using Domain.Dtos;

namespace Application.Interfaces;

public interface IInvoiceService
{
    Task<string> EnqueueInvoicesAsync(BulkInvoiceRequest req);
}
