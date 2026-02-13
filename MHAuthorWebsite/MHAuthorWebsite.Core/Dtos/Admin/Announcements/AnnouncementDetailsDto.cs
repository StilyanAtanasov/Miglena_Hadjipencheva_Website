using MHAuthorWebsite.Core.Models.Enums;

namespace MHAuthorWebsite.Core.Dtos.Admin.Announcements;

public class AnnouncementDetailsDto
{
    public Guid Id { get; set; }

    public string Subject { get; set; } = null!;

    public string MessageDelta { get; set; } = null!;

    public AnnouncementRecipientGroup RecipientGroup { get; set; }

    public string? AdditionalRecipients { get; set; }

    public int RecipientCount { get; set; }

    public DateTime CreatedOn { get; set; }

    public string AdminName { get; set; } = null!;
}
