using System.ComponentModel.DataAnnotations;
using static MHAuthorWebsite.GCommon.EntityConstraints.Work;

namespace MHAuthorWebsite.Core.Models;

public class Work
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(TitleMaxLength)]
    public string Title { get; set; } = null!;

    /// <summary>
    /// Rich text content of the work/post (HTML including formatting).
    /// </summary>
    [Required]
    public string Content { get; set; } = null!;

    [Required]
    public string CoverImageUrl { get; set; } = null!;

    [Required]
    public string CoverImagePublicId { get; set; } = null!; // For Cloudinary management

    public DateTime DatePublished { get; set; }

    public bool IsPublic { get; set; } = true;
}
