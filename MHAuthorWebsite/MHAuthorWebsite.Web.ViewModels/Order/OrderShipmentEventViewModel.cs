namespace MHAuthorWebsite.Web.ViewModels.Order;

public class OrderShipmentEventViewModel
{
    public DateTime Time { get; set; }

    public string DestinationDetails { get; set; } = null!;

    public string? OfficeName { get; set; }

    public string? CityName { get; set; }
}