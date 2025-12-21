namespace MHAuthorWebsite.Core.Dtos.Order;

public class OrderProductDetailsDto
{
    public string ImageUrl { get; set; } = null!;

    public string ProductName { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}