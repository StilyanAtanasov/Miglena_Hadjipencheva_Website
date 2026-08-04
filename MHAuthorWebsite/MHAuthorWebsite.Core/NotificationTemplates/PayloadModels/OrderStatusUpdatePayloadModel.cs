namespace MHAuthorWebsite.Core.NotificationTemplates.PayloadModels;

public class OrderStatusUpdatePayloadModel
{
    public string CustomerName { get; set; } = null!;

    public Guid OrderId { get; set; }

    public string OrderNumber { get; set; } = null!;

    public string? ShipmentNumber { get; set; }

    public string StatusUpdate { get; set; } = null!;

    public string Location { get; set; } = null!;

    public string? LocationDetails { get; set; }

    public string EventTime { get; set; } = null!;

    public string? TrackingUrl { get; set; }
}