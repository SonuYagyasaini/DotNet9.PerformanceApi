namespace Domain.Entities;

public class Invoice
{
    public int Id { get; set; }
    public string Customer { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime CreatedOn { get; set; }
}
