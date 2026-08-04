using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MHAuthorWebsite.Core.Models;

public class AdminNotificationPreference
{
    [Key]
    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = null!;

    public ApplicationUser User { get; set; } = null!;

    public bool ReceiveNewOrderEmails { get; set; } = true;

    public bool ReceiveContactRequestEmails { get; set; } = true;

    public bool ReceiveServerErrorEmails { get; set; } = true;

    public DateTime UpdatedOn { get; set; } = DateTime.UtcNow;
}
