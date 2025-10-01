using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static MHAuthorWebsite.GCommon.EntityConstraints.ProductCommentImage;

namespace MHAuthorWebsite.Data.Models;

public class ProductCommentImage
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
    [Comment("Foreign key to Comment")]
    [ForeignKey(nameof(Comment))]
    public Guid CommentId { get; set; }

    public ProductComment Comment { get; set; } = null!;
}