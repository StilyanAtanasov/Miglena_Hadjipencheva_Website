using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static MHAuthorWebsite.GCommon.EntityConstraints.Product;

namespace MHAuthorWebsite.Core.Models;

public class Product
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(NameMaxLength)]
    public string Name { get; set; } = null!;

    [Required]
    [MaxLength(DescriptionDeltaMaxLength)]
    public string Description { get; set; } = null!;

    [Required]
    [Column(TypeName = PriceSqlType)]
    public decimal Price { get; set; }

    [Required]
    public int StockQuantity { get; set; }

    [Required]
    [Column(TypeName = WeightSqlType)]
    public decimal Weight { get; set; }

    [Required]
    [ForeignKey(nameof(ProductType))]
    public int ProductTypeId { get; set; }

    public ProductType ProductType { get; set; } = null!;

    public ProductThumbnail Thumbnail { get; set; } = null!;

    public ICollection<ProductImage> Images { get; set; } = new HashSet<ProductImage>();

    public ICollection<ProductAttribute> Attributes { get; set; } = new HashSet<ProductAttribute>();

    public ICollection<ProductComment> Comments { get; set; } = new HashSet<ProductComment>();

    public ICollection<ApplicationUser> Likes { get; set; } = new HashSet<ApplicationUser>();

    public ICollection<OrderProduct> Orders { get; set; } = new HashSet<OrderProduct>();

    public ICollection<ProductDiscount> Discounts { get; set; } = new HashSet<ProductDiscount>();

    public bool IsPublic { get; set; }

    public bool IsDeleted { get; set; }
}