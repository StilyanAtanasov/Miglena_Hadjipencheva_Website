using MHAuthorWebsite.Core.Models.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MHAuthorWebsite.Core.Models;

public class ScheduledNotification
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public ScheduledNotificationType NotificationType { get; set; }

    [Required]
    public ScheduledNotificationStatus NotificationStatus { get; set; }

    [Required]
    public ScheduledNotificationTemplate NotificationTemplate { get; set; }

    public string? Payload { get; set; }

    [ForeignKey(nameof(Recipient))]
    public string RecipientId { get; set; } = null!;

    public IdentityUser Recipient { get; set; } = null!;

    [Required]
    public DateTime ScheduledAt { get; set; }

    public DateTime? SentAt { get; set; }
}