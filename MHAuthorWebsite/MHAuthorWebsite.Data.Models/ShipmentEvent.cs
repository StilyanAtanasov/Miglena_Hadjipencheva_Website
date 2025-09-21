using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static MHAuthorWebsite.GCommon.EntityConstraints.ShipmentEvent;

namespace MHAuthorWebsite.Data.Models;

public class ShipmentEvent
{
    [Key]
    public Guid Id { get; set; }

    public DateTime Time { get; set; }

    [MaxLength(DestinationTypeMaxLength)]
    public string? DestinationType { get; set; }

    [MaxLength(DestinationDetailsMaxLength)]
    public string? DestinationDetails { get; set; }

    [MaxLength(OfficeNameMaxLength)]
    public string? OfficeName { get; set; }

    [MaxLength(CityNameMaxLength)]
    public string? CityName { get; set; }

    [Required]
    [ForeignKey(nameof(Shipment))]
    public Guid ShipmentId { get; set; }

    public Shipment Shipment { get; set; } = null!;
}