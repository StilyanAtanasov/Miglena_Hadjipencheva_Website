using MHAuthorWebsite.Core.Models.Enums;

namespace MHAuthorWebsite.Core.Dtos.Admin.Announcements;

public class CreateAnnouncementDto
{
    public string Subject { get; set; } = null!;

    public string MessageDelta { get; set; } = null!;

    public AnnouncementRecipientGroup RecipientGroup { get; set; }

    public string? AdditionalRecipients { get; set; }
}
