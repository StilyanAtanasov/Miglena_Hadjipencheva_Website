namespace MHAuthorWebsite.Web.ViewModels.Order;

public class OrderDetailsViewModel
{
    public Guid OrderId { get; set; }

    public DateTime OrderDate { get; set; }

    public string Status { get; set; } = null!;

    public OrderShipmentDetailsViewModel Shipment { get; set; } = null!;

    public ICollection<OrderProductDetailsViewModel> Products { get; set; } = null!;
}