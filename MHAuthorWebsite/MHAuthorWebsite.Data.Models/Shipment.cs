using MHAuthorWebsite.Data.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static MHAuthorWebsite.GCommon.EntityConstraints.Shipment;

namespace MHAuthorWebsite.Data.Models;

public class Shipment
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Order))]
    public Guid OrderId { get; set; }

    public Order Order { get; set; } = null!;

    [MaxLength(OrderNumberMaxLength)]
    public string? OrderNumber { get; set; }

    [MaxLength(ShipmentNumberMaxLength)]
    public string? ShipmentNumber { get; set; }

    [Required]
    [MaxLength(PhoneMaxLength)]
    public string Phone { get; set; } = null!;

    [Required]
    [MaxLength(FaceMaxLength)]
    public string Face { get; set; } = null!;

    [Required]
    [MaxLength(EmailMaxLength)]
    public string Email { get; set; } = null!;

    [Required]
    public Courier Courier { get; set; }

    /*[MaxLength(50)]
    public string ShippingStatus { get; set; } = "Pending";*/ // TODO Add enum for shipping status

    [Column(TypeName = ShippingPriceSqlType)]
    public decimal ShippingPrice { get; set; }

    [Required]
    [MaxLength(CurrencyMaxLength)]
    public string Currency { get; set; } = null!;

    [MaxLength(AddressMaxLength)]
    public string? Address { get; set; }

    [MaxLength(CityMaxLength)]
    public string? City { get; set; }

    [MaxLength(PostCodeMaxLength)]
    public string? PostCode { get; set; }

    [MaxLength(PriorityFromMaxLength)]
    public string? PriorityFrom { get; set; }

    [MaxLength(PriorityToMaxLength)]
    public string? PriorityTo { get; set; }

    [MaxLength(ShipmentDescriptionMaxLength)]
    public string? ShipmentDescription { get; set; }
}