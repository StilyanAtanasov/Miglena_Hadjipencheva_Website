using System.ComponentModel.DataAnnotations;

namespace MHAuthorWebsite.Core.Models.Enums;

public enum AnnouncementRecipientSource
{
    [Display(Name = "Абониран потребител")]
    SubscribedUser = 1,

    [Display(Name = "Абониран администратор")]
    Admin = 2,

    [Display(Name = "Допълнителен имейл")]
    AdditionalEmail = 3
}
