using MHAuthorWebsite.Core.Models.Enums;

namespace MHAuthorWebsite.Core.Dtos.Admin.Order;

public class AdminOrderDetailsDto
{
    public Guid OrderId { get; set; }

    public DateTime OrderDate { get; set; }

    public OrderStatus Status { get; set; }

    public AdminOrderShipmentDetailsDto Shipment { get; set; } = null!;

    public ICollection<AdminOrderProductDetailsDto> Products { get; set; } = null!;
}