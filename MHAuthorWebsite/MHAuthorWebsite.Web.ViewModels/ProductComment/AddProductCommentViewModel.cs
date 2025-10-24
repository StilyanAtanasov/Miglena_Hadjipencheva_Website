using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using static MHAuthorWebsite.GCommon.EntityConstraints.ProductComment;

namespace MHAuthorWebsite.Web.ViewModels.ProductComment;

public class AddProductCommentViewModel
{
    public Guid ProductId { get; set; }

    public Guid? ParentCommentId { get; set; }

    public Guid? ReplyCommentId { get; set; }

    [Range(RatingMinValue, RatingMaxValue)]
    public short? Rating { get; set; }

    [Required]
    [StringLength(TextMaxLength, MinimumLength = TextMinLength)]
    public string Text { get; set; } = null!;

    public string TargetName { get; set; } = null!;

    public ICollection<IFormFile>? Images { get; set; }
}