namespace MHAuthorWebsite.Web.ViewModels.Admin.Order;

public class AdminOrderDetailsViewModel
{
    public Guid OrderId { get; set; }

    public DateTime OrderDate { get; set; }

    public string Status { get; set; } = null!;

    public AdminOrderShipmentDetailsViewModel Shipment { get; set; } = null!;

    public ICollection<AdminOrderProductDetailsViewModel> Products { get; set; } = null!;
}