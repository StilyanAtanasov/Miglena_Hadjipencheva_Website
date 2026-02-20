using System.ComponentModel.DataAnnotations;

namespace MHAuthorWebsite.Core.Models.Enums;

public enum AnnouncementRecipientGroup
{
    [Display(Name = "Всички абонирани потребители")]
    SubscribedUsers = 1,

    [Display(Name = "Само абонирани администратори")]
    Admins = 2,

    [Display(Name = "Абонирани потребители и администратори")]
    SubscribedUsersAndAdmins = 3,

    [Display(Name = "Само допълнителни имейли")]
    AdditionalRecipientsOnly = 4
}

