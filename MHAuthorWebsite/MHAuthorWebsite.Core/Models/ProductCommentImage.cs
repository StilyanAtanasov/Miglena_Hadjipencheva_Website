using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static MHAuthorWebsite.GCommon.EntityConstraints.ProductCommentImage;

namespace MHAuthorWebsite.Core.Models;

public class ProductCommentImage
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
    [MaxLength(UrlMaxLength)]
    public string PreviewUrl { get; set; } = null!;

    [Required]
    public string PreviewPublicId { get; set; } = null!;

    [Required]
    [ForeignKey(nameof(Comment))]
    public Guid CommentId { get; set; }

    public ProductComment Comment { get; set; } = null!;
}