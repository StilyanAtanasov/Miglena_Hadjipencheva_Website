namespace MHAuthorWebsite.Core.Dtos.Order;

public class MyOrderDto
{
    public Guid OrderId { get; set; }

    public DateTime CreatedAt { get; set; }

    public decimal Total { get; set; }

    public string Currency { get; set; } = null!;

    public string Status { get; set; } = null!;

    public ICollection<MyOrdersOrderProductDto> Products { get; set; } = null!;
}
