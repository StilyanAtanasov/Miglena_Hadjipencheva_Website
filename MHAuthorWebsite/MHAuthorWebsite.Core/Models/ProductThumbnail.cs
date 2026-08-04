using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MHAuthorWebsite.Core.Models;

public class ProductThumbnail
{
    [Required]
    [ForeignKey(nameof(Image))]
    public Guid ImageId { get; set; }

    public ProductImage Image { get; set; } = null!;

    [Required]
    [ForeignKey(nameof(ImageOriginal))]
    public Guid ImageOriginalId { get; set; }

    public ProductImage ImageOriginal { get; set; } = null!;

    [Required]
    [ForeignKey(nameof(Product))]
    public Guid ProductId { get; set; }

    public Product Product { get; set; } = null!;
}
