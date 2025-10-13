namespace MHAuthorWebsite.Web.ViewModels.Admin.Order;

public class AdminOrderProductDetailsViewModel
{
    public Guid Id { get; set; }

    public string ImageUrl { get; set; } = null!;

    public string ProductName { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}