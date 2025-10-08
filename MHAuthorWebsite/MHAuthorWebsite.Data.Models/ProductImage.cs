using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static MHAuthorWebsite.GCommon.EntityConstraints.ProductImage;

namespace MHAuthorWebsite.Data.Models;

public class ProductImage
{
    [Key]
    [Comment("Primary key")]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(UrlMaxLength)]
    [Comment("URL path to the image")]
    public string ImageUrl { get; set; } = null!;

    [Required]
    [Comment("The publicId in Cloudinary")]
    public string PublicId { get; set; } = null!;

    [Required]
    [MaxLength(AltTextMaxLength)]
    [Comment("Alternative text for accessibility")]
    public string AltText { get; set; } = null!;

    [Required]
    [Comment("Foreign key to Product")]
    [ForeignKey(nameof(Product))]
    public Guid ProductId { get; set; }

    public Product Product { get; set; } = null!;
}