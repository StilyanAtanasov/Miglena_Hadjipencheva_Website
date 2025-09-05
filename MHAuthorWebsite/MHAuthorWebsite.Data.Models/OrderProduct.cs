using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static MHAuthorWebsite.GCommon.EntityConstraints.OrderProduct;

namespace MHAuthorWebsite.Data.Models;

[PrimaryKey(nameof(ProductId), nameof(OrderId))]
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
}