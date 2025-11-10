using MHAuthorWebsite.Web.Common.Localization;
using System.ComponentModel.DataAnnotations;
using static MHAuthorWebsite.GCommon.EntityConstraints.ContactRequest;

namespace MHAuthorWebsite.Web.ViewModels.Admin.ContactRequests;

public class ContactRequestDetailsViewModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Subject { get; set; } = null!;

    public string Message { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    public DateTime? RepliedOn { get; set; }

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Required")]
    [StringLength(ReplyMessageMaxLength, MinimumLength = ReplyMessageMinLength, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "StringLength")]
    public string ReplyMessage { get; set; } = null!;

    public bool IsAnswered { get; set; }
}