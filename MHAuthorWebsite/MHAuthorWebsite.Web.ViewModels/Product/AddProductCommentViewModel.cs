using System.ComponentModel.DataAnnotations;

namespace MHAuthorWebsite.Web.ViewModels.Product;
using static GCommon.EntityConstraints.ProductComment;

public class AddProductCommentViewModel
{
    public Guid ProductId { get; set; }

    public Guid? ParentCommentId { get; set; }

    [Range(RatingMinValue, RatingMaxValue)]
    public short? Rating { get; set; }

    [Required]
    [StringLength(TextMaxLength, MinimumLength = TextMinLength)]
    public string Text { get; set; } = null!;
}