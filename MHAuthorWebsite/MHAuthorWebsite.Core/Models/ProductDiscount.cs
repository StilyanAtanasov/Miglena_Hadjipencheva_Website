using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static MHAuthorWebsite.GCommon.EntityConstraints.ProductDiscount;
using static MHAuthorWebsite.GCommon.EntityConstraints.Shipment;

namespace MHAuthorWebsite.Core.Models;

public class ProductDiscount
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Product))]
    public Guid ProductId { get; set; }

    public Product Product { get; set; } = null!;

    [Required]
    [Column(TypeName = NewPriceSqlType)]
    public decimal NewPrice { get; set; }

    [Required]
    [MaxLength(CurrencyMaxLength)]
    public string Currency { get; set; } = null!;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }
}
