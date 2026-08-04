using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static MHAuthorWebsite.GCommon.EntityConstraints.ProductComment;

namespace MHAuthorWebsite.Core.Models;

public class ProductComment
{
    [Key]
    public Guid Id { get; set; }

    [Range(RatingMinValue, RatingMaxValue)]
    public short? Rating { get; set; }

    [Required]
    [MaxLength(TextMaxLength)]
    public string Text { get; set; } = null!;

    [Required]
    public DateTime Date { get; set; }

    public DateTime? LastEdited { get; set; }

    [Required]
    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = null!;

    public ApplicationUser User { get; set; } = null!;

    [Required]
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

    public bool IsDeleted { get; set; }

    public ICollection<ProductComment> Replies { get; set; } = new HashSet<ProductComment>();

    public ICollection<ProductCommentImage> Images { get; set; } = new HashSet<ProductCommentImage>();

    public ICollection<ProductCommentReaction> Reactions { get; set; } = new HashSet<ProductCommentReaction>();
}