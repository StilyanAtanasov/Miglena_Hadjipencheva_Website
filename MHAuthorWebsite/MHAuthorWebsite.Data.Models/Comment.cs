using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static MHAuthorWebsite.GCommon.EntityConstraints.Comment;

namespace MHAuthorWebsite.Data.Models;

public class Comment
{
    [Key]
    [Comment("Primary key")]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(TextMaxLength)]
    [Comment("User comment")]
    public string Text { get; set; } = null!;

    [Required]
    [Comment("Timestamp")]
    public DateTime Date { get; set; }

    [Required]
    [Comment("Foreign key to User")]
    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = null!;

    public IdentityUser User { get; set; } = null!;

    [Required]
    [Comment("Foreign key to Product")]
    [ForeignKey(nameof(Product))]
    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;

    [Comment("Soft delete flag")]
    public bool IsDeleted { get; set; }
}