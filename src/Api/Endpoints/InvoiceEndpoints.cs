using Application.Interfaces;
using Domain.Dtos;

namespace Api.Endpoints;

public static class InvoiceEndpoints
{
    public static void MapInvoiceEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/invoice/bulk", async (BulkInvoiceRequest req, IInvoiceService service) =>
        {
            if (req?.Data == null || req.Data.Count == 0) return Results.BadRequest("No data");
            var res = await service.EnqueueInvoicesAsync(req);
            return Results.Accepted(value: new { message = res });
        })
        .WithName("Bulk Insert Invoice")
        .RequireRateLimiting("fixed");
    }
}
