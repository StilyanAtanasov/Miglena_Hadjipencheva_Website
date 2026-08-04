namespace MHAuthorWebsite.Core.Dtos.Admin;

public class AdminNotificationPreferencesDto
{
    public bool ReceiveNewOrderEmails { get; set; }

    public bool ReceiveContactRequestEmails { get; set; }

    public bool ReceiveServerErrorEmails { get; set; }
}
