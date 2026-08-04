using MHAuthorWebsite.Core.Models.Enums;

namespace MHAuthorWebsite.Core.Dtos.Admin.Announcements;

public class AnnouncementListItemDto
{
    public Guid Id { get; set; }

    public string Subject { get; set; } = null!;

    public string MessagePreview { get; set; } = null!;

    public AnnouncementRecipientGroup RecipientGroup { get; set; }

    public int RecipientCount { get; set; }

    public DateTime CreatedOn { get; set; }

    public string AdminName { get; set; } = null!;
}
