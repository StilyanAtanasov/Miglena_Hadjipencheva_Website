using MHAuthorWebsite.Core.Models.Enums;
using MHAuthorWebsite.Web.Common.Localization;
using System.ComponentModel.DataAnnotations;
using static MHAuthorWebsite.GCommon.EntityConstraints.Announcement;

namespace MHAuthorWebsite.Web.ViewModels.Admin.Announcements;

public class AddAnnouncementFormViewModel
{
    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Required")]
    [StringLength(SubjectMaxLength, MinimumLength = SubjectMinLength, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "StringLength")]
    public string Subject { get; set; } = null!;

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Required")]
    public string MessageDelta { get; set; } = null!;

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Required")]
    [MaxLength(MessageHtmlMaxLength, ErrorMessage = "HTML съдържанието е прекалено голямо.")]
    public string MessageHtml { get; set; } = null!;

    [Range(1, 4, ErrorMessage = "Изберете аудитория.")]
    public AnnouncementRecipientGroup RecipientGroup { get; set; }

    [MaxLength(AdditionalRecipientsMaxLength, ErrorMessage = "Полето с допълнителни имейли е прекалено дълго.")]
    public string? AdditionalRecipients { get; set; }
}

