namespace MHAuthorWebsite.Core.Dtos.Order;

public class OrderDetailsDto
{
    public Guid OrderId { get; set; }

    public DateTime OrderDate { get; set; }

    public string Status { get; set; } = null!;

    public OrderShipmentDetailsDto Shipment { get; set; } = null!;

    public ICollection<OrderProductDetailsDto> Products { get; set; } = null!;
}