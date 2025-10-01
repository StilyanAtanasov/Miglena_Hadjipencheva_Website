using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MHAuthorWebsite.Data.Models;

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
    [Comment("Reaction type")]
    public Enums.CommentReaction Reaction { get; set; }

    [Required]
    [Comment("Time when the reaction was made")]
    public DateTime CreatedAt { get; set; }
}