using MHAuthorWebsite.Core.Models.Enums;

namespace MHAuthorWebsite.Core.Dtos.Admin.Announcements;

public class AnnouncementRecipientDeliveryDto
{
    public string Email { get; set; } = null!;

    public AnnouncementRecipientSource RecipientSource { get; set; }

    public AnnouncementDeliveryStatus DeliveryStatus { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime? DeliveredOn { get; set; }
}
