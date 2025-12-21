namespace MHAuthorWebsite.Core.Dtos.Order;

public class OrderItemDto
{
    public string Name { get; set; } = null!;

    public int Count { get; set; }

    public decimal TotalPrice { get; set; }

    public decimal TotalWeight { get; set; }
}