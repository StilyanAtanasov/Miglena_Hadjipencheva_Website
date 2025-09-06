namespace MHAuthorWebsite.Core.Dto;

public class EcontOrderDto
{
    public long? Id { get; set; }

    public string? OrderNumber { get; set; }

    public string? Status { get; set; }

    public long? OrderTime { get; set; }

    public decimal OrderSum { get; set; }

    public bool Cod { get; set; } = true;

    public bool PartialDelivery { get; set; } = false;

    public string Currency { get; set; } = "BGN";

    public string? ShipmentDescription { get; set; }

    public string? ShipmentNumber { get; set; }

    public CustomerInfoDto CustomerInfo { get; set; } = null!;

    public ICollection<OrderItemDto> Items { get; set; } = null!;
}