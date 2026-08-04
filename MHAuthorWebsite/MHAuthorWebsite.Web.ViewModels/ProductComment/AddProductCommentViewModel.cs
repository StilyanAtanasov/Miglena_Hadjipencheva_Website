using MHAuthorWebsite.Web.Common.Localization;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using static MHAuthorWebsite.GCommon.EntityConstraints.ProductComment;

namespace MHAuthorWebsite.Web.ViewModels.ProductComment;

public class AddProductCommentViewModel
{
    public Guid ProductId { get; set; }

    public Guid? ParentCommentId { get; set; }

    public Guid? ReplyCommentId { get; set; }

    [Range(RatingMinValue, RatingMaxValue, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Range")]
    public short? Rating { get; set; }

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Required")]
    [StringLength(TextMaxLength, MinimumLength = TextMinLength, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "StringLength")]
    public string Text { get; set; } = null!;

    public string TargetName { get; set; } = null!;

    public ICollection<IFormFile>? Images { get; set; }
}