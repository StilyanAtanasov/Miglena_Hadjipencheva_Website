using System.ComponentModel.DataAnnotations;

namespace MHAuthorWebsite.Core.Models.Enums;

public enum AnnouncementRecipientGroup
{
    [Display(Name = "Всички потребители")]
    SubscribedUsers = 1,

    [Display(Name = "Само администратори")]
    Admins = 2,

    [Display(Name = "Потребители и администратори")]
    SubscribedUsersAndAdmins = 3,

    [Display(Name = "Само допълнителни имейли")]
    AdditionalRecipientsOnly = 4
}

