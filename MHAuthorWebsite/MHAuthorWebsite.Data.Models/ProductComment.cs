using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static MHAuthorWebsite.GCommon.EntityConstraints.ProductComment;

namespace MHAuthorWebsite.Data.Models;

public class ProductComment
{
    [Key]
    [Comment("Primary key")]
    public Guid Id { get; set; }

    [Range(RatingMinValue, RatingMaxValue)]
    [Comment("User rating, but null since replies do not share rating!")]
    public short? Rating { get; set; }

    [Required]
    [MaxLength(TextMaxLength)]
    [Comment("User comment")]
    public string Text { get; set; } = null!;

    [Required]
    [Comment("Created at")]
    public DateTime Date { get; set; }

    [Required]
    [Comment("Foreign key to User")]
    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = null!;

    public ApplicationUser User { get; set; } = null!;

    [Required]
    [Comment("Foreign key to Product")]
    [ForeignKey(nameof(Product))]
    public Guid ProductId { get; set; }

    public Product Product { get; set; } = null!;

    [ForeignKey(nameof(ParentComment))]
    public Guid? ParentCommentId { get; set; }

    public ProductComment? ParentComment { get; set; }

    [ForeignKey(nameof(ParentReply))]
    public Guid? ParentReplyId { get; set; }

    public ProductComment? ParentReply { get; set; }

    [Required]
    public bool VerifiedPurchase { get; set; }

    [Comment("Soft delete flag")]
    public bool IsDeleted { get; set; }

    public ICollection<ProductComment> Replies { get; set; } = new HashSet<ProductComment>();

    public ICollection<ProductCommentImage> Images { get; set; } = new HashSet<ProductCommentImage>();

    public ICollection<ProductCommentReaction> Reactions { get; set; } = new HashSet<ProductCommentReaction>();
}