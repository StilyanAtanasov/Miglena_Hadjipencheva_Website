using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static MHAuthorWebsite.GCommon.EntityConstraints.OrderProduct;
using static MHAuthorWebsite.GCommon.EntityConstraints.Shipment;

namespace MHAuthorWebsite.Core.Models;

public class OrderProduct
{
    [Required]
    [ForeignKey(nameof(Order))]
    public Guid OrderId { get; set; }

    public Order Order { get; set; } = null!;

    [Required]
    [ForeignKey(nameof(Product))]
    public Guid ProductId { get; set; }

    public Product Product { get; set; } = null!;

    [Required]
    public int Quantity { get; set; }

    [Column(TypeName = UnitPriceSqlType)]
    public decimal UnitPrice { get; set; }

    [Required]
    [MaxLength(CurrencyMaxLength)]
    public string Currency { get; set; } = null!;
}
