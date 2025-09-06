using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static MHAuthorWebsite.GCommon.EntityConstraints.Product;

namespace MHAuthorWebsite.Data.Models;

public class Product
{
    [Key]
    [Comment("Primary key")]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(NameMaxLength)]
    [Comment("Product name")]
    public string Name { get; set; } = null!;

    [Required]
    [MaxLength(DescriptionDeltaMaxLength)]
    [Comment("Product description")]
    public string Description { get; set; } = null!;

    [Required]
    [Column(TypeName = PriceSqlType)]
    [Comment("Unit price")]
    public decimal Price { get; set; }

    [Required]
    [Comment("Quantity in stock")]
    public int StockQuantity { get; set; }

    [Required]
    [Comment("Product's weight")]
    [Column(TypeName = WeightSqlType)]
    public decimal Weight { get; set; }

    [Required]
    [Comment("Foreign key to ProductType")]
    [ForeignKey(nameof(ProductType))]
    public int ProductTypeId { get; set; }

    public ProductType ProductType { get; set; } = null!;

    public ICollection<Image> Images { get; set; } = new HashSet<Image>();

    public ICollection<ProductAttribute> Attributes { get; set; } = new HashSet<ProductAttribute>();

    public ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();

    public ICollection<IdentityUser> Likes { get; set; } = new HashSet<IdentityUser>();

    public ICollection<OrderProduct> Orders { get; set; } = new HashSet<OrderProduct>();

    [Comment("Determines if the product should be visible for basic users")]
    public bool IsPublic { get; set; }

    [Comment("Soft delete flag")]
    public bool IsDeleted { get; set; }
}