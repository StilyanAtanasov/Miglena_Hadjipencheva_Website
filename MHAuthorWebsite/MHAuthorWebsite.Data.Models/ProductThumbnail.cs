using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MHAuthorWebsite.Data.Models;

[PrimaryKey(nameof(ImageId), nameof(ProductId))]
public class ProductThumbnail
{
    [Required]
    [Comment("Foreign key to ProductImage")]
    [ForeignKey(nameof(Image))]
    public Guid ImageId { get; set; }

    public ProductImage Image { get; set; } = null!;

    [Required]
    [Comment("Foreign key to the original image")]
    [ForeignKey(nameof(ImageOriginal))]
    public Guid ImageOriginalId { get; set; }

    public ProductImage ImageOriginal { get; set; } = null!;

    [Required]
    [Comment("Foreign key to Product")]
    [ForeignKey(nameof(Product))]
    public Guid ProductId { get; set; }

    public Product Product { get; set; } = null!;
}
