using MHAuthorWebsite.Core.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static MHAuthorWebsite.GCommon.EntityConstraints.ScheduledNotification;

namespace MHAuthorWebsite.Core.Models;

public class ScheduledNotification
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(SubjectMaxLength)]
    public string Subject { get; set; } = null!;

    [Required]
    public ScheduledNotificationType NotificationType { get; set; }

    [Required]
    public ScheduledNotificationStatus NotificationStatus { get; set; }

    [Required]
    public ScheduledNotificationTemplate NotificationTemplate { get; set; }

    public string? Payload { get; set; }

    [ForeignKey(nameof(Recipient))]
    public string RecipientId { get; set; } = null!;

    public ApplicationUser Recipient { get; set; } = null!;

    [MaxLength(TargetDeliveryDetailsMaxLength)]
    public string? TargetDeliveryDetails { get; set; }

    [Required]
    public DateTime ScheduledAt { get; set; }

    public DateTime? SentAt { get; set; }

    [Required]
    public DateTime ExpirationDate { get; set; }
}