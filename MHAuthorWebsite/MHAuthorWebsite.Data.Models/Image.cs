using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static MHAuthorWebsite.GCommon.EntityConstraints.Image;

namespace MHAuthorWebsite.Data.Models;
public class Image
{
    [Key]
    [Comment("Primary key")]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(UrlMaxLength)]
    [Comment("URL path to the image")]
    public string ImageUrl { get; set; } = null!;

    [MaxLength(UrlMaxLength)]
    [Comment("URL path to the thumbnail")]
    public string? ThumbnailUrl { get; set; } = null!;

    [Required]
    [Comment("The publicId in Cloudinary")]
    public string PublicId { get; set; } = null!;

    [Comment("The publicId for thumbnail in Cloudinary")]
    public string? ThumbnailPublicId { get; set; }

    [Required]
    [MaxLength(AltTextMaxLength)]
    [Comment("Alternative text for accessibility")]
    public string AltText { get; set; } = null!;

    [Required]
    [Comment("Defines whether the image is a thumbnail (the image will be used for product listings)")]
    public bool IsThumbnail { get; set; }

    [Required]
    [Comment("Foreign key to Product")]
    [ForeignKey(nameof(Product))]
    public Guid ProductId { get; set; }

    public Product Product { get; set; } = null!;
}