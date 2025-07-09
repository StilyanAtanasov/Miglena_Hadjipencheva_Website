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

    [MaxLength(AltTextMaxLength)]
    [Comment("Alternative text for accessibility")]
    public string? AltText { get; set; }

    [Required]
    [Comment("Foreign key to Product")]
    [ForeignKey(nameof(Product))]
    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;
}