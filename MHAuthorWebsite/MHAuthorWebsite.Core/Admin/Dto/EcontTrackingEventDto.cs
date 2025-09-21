namespace MHAuthorWebsite.Core.Admin.Dto;

public class EcontTrackingEventDto
{
    public string? Time { get; set; }

    public bool IsReceipt { get; set; }

    public string? DestinationType { get; set; }

    public string? DestinationDetails { get; set; }

    public string? DestinationDetailsEn { get; set; }

    public string? OfficeName { get; set; }

    public string? OfficeNameEn { get; set; }

    public string? CityName { get; set; }

    public string? CityNameEn { get; set; }

    public string? CountryCode { get; set; }
}