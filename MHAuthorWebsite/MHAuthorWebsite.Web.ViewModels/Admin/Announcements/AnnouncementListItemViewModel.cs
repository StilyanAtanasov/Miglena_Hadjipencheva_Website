namespace MHAuthorWebsite.Web.ViewModels.Admin.Announcements;

public class AnnouncementListItemViewModel
{
    public Guid Id { get; set; }

    public string Subject { get; set; } = null!;

    public string MessagePreview { get; set; } = null!;

    public string RecipientGroupLabel { get; set; } = null!;

    public int RecipientCount { get; set; }

    public DateTime CreatedOn { get; set; }

    public string AdminName { get; set; } = null!;
}
