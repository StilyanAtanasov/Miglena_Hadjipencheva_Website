using System.ComponentModel.DataAnnotations;
using static MHAuthorWebsite.GCommon.EntityConstraints.ContactRequest;
namespace MHAuthorWebsite.Web.ViewModels.Contacts;

public class ContactFormViewModel
{
    [Required]
    [StringLength(NameMaxLength, MinimumLength = NameMinLength)]
    public string Name { get; set; } = null!;

    [Required]
    [StringLength(EmailMaxLength, MinimumLength = EmailMinLength)]
    public string Email { get; set; } = null!;

    [Required]
    [StringLength(SubjectMaxLength, MinimumLength = SubjectMinLength)]
    public string Subject { get; set; } = null!;

    [Required]
    [StringLength(MessageMaxLength, MinimumLength = MessageMinLength)]
    public string Message { get; set; } = null!;
}