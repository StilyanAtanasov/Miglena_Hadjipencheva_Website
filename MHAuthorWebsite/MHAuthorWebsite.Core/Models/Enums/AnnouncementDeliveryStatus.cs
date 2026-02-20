using System.ComponentModel.DataAnnotations;

namespace MHAuthorWebsite.Core.Models.Enums;

public enum AnnouncementDeliveryStatus
{
    [Display(Name = "Изпратен")]
    Sent = 1,

    [Display(Name = "Неуспешен")]
    Failed = 2
}
