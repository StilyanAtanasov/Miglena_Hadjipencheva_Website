namespace MHAuthorWebsite.Web.ViewModels.Admin.Announcements;

public class AnnouncementDetailsViewModel
{
    public Guid Id { get; set; }

    public string Subject { get; set; } = null!;

    public string MessageDelta { get; set; } = null!;

    public string RecipientGroupLabel { get; set; } = null!;

    public string? AdditionalRecipients { get; set; }

    public int RecipientCount { get; set; }

    public int FailedRecipientCount { get; set; }

    public ICollection<AnnouncementRecipientDeliveryViewModel> Deliveries { get; set; } = Array.Empty<AnnouncementRecipientDeliveryViewModel>();

    public DateTime CreatedOn { get; set; }

    public string AdminName { get; set; } = null!;
}
