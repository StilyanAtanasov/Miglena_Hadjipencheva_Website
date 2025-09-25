namespace MHAuthorWebsite.Web.ViewModels.Admin.Order;

public class AdminOrderShipmentServiceViewModel
{
    public string Type { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int Count { get; set; }

    public string PaymentSide { get; set; } = null!;

    public decimal Price { get; set; }

    public string Currency { get; set; } = null!;
}