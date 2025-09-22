namespace MHAuthorWebsite.Web.ViewModels.Order;

public class OrderProductDetailsViewModel
{
    public string ImageUrl { get; set; } = null!;

    public string ProductName { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}