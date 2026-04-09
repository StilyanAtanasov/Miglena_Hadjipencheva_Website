using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static MHAuthorWebsite.GCommon.EntityConstraints.Product;
using static MHAuthorWebsite.GCommon.EntityConstraints.Shipment;

namespace MHAuthorWebsite.Core.Models;

public class CartItem
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Product))]
    public Guid ProductId { get; set; }

    [Required]
    public Product Product { get; set; } = null!;

    [Required]
    public int Quantity { get; set; }

    [Required]
    [Column(TypeName = PriceSqlType)]
    public decimal Price { get; set; }

    [Required]
    [MaxLength(CurrencyMaxLength)]
    public string Currency { get; set; } = null!;

    [Required]
    public bool IsSelected { get; set; }

    [Required]
    [ForeignKey(nameof(Cart))]
    public Guid CartId { get; set; }

    [Required]
    public Cart Cart { get; set; } = null!;
}
