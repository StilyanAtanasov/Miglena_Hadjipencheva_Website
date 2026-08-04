using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static MHAuthorWebsite.GCommon.EntityConstraints.ProductImage;

namespace MHAuthorWebsite.Core.Models;

public class ProductImage
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(UrlMaxLength)]
    public string ImageUrl { get; set; } = null!;

    [Required]
    public string PublicId { get; set; } = null!;

    [Required]
    [MaxLength(AltTextMaxLength)]
    public string AltText { get; set; } = null!;

    [Required]
    [ForeignKey(nameof(Product))]
    public Guid ProductId { get; set; }

    public Product Product { get; set; } = null!;
}