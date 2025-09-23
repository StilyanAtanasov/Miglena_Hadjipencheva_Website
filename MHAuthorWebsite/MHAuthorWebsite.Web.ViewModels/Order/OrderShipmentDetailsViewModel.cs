namespace MHAuthorWebsite.Web.ViewModels.Order;

public class OrderShipmentDetailsViewModel
{
    public string? ShipmentNumber { get; set; }

    public string CourierName { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Face { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? PostCode { get; set; }

    public decimal ShippingPrice { get; set; }

    public string Currency { get; set; } = null!;

    public string? PriorityFrom { get; set; }

    public string? PriorityTo { get; set; }

    public DateTime? ExpectedDeliveryDate { get; set; }

    public ICollection<OrderShipmentEventViewModel> TrackingEvents { get; set; } = null!;
}