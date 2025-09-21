using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static MHAuthorWebsite.GCommon.EntityConstraints.ShipmentService;

namespace MHAuthorWebsite.Data.Models;

public class ShipmentService
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(TypeMaxLength)]
    public string Type { get; set; } = null!;

    [MaxLength(DescriptionMaxLength)]
    public string Description { get; set; } = null!;

    public int Count { get; set; }

    [MaxLength(PaymentSideMaxLength)]
    public string PaymentSide { get; set; } = null!;

    [Column(TypeName = PriceSqlType)]
    public decimal Price { get; set; }

    [MaxLength(CurrentMaxLength)]
    public string Currency { get; set; } = null!;

    [Required]
    [ForeignKey(nameof(Shipment))]
    public Guid ShipmentId { get; set; }

    public Shipment Shipment { get; set; } = null!;
}