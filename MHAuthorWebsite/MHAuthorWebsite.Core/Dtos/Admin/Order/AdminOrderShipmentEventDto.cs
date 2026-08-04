namespace MHAuthorWebsite.Core.Dtos.Admin.Order;

public class AdminOrderShipmentEventDto
{
    public DateTime Time { get; set; }

    public string DestinationDetails { get; set; } = null!;

    public string? OfficeName { get; set; }

    public string? CityName { get; set; }
}