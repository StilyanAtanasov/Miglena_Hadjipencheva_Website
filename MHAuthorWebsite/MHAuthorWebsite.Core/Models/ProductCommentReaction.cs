using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MHAuthorWebsite.Core.Models;

public class ProductCommentReaction
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Comment))]
    public Guid CommentId { get; set; }

    public ProductComment Comment { get; set; } = null!;

    [Required]
    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = null!;

    public ApplicationUser User { get; set; } = null!;

    [Required]
    public Enums.CommentReaction Reaction { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }
}