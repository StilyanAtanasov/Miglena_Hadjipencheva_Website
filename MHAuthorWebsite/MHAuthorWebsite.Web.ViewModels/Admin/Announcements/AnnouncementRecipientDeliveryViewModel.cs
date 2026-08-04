namespace MHAuthorWebsite.Web.ViewModels.Admin.Announcements;

public class AnnouncementRecipientDeliveryViewModel
{
    public string Email { get; set; } = null!;

    public string RecipientSourceLabel { get; set; } = null!;

    public bool IsDelivered { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime? DeliveredOn { get; set; }
}
