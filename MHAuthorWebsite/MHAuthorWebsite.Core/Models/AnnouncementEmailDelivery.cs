using MHAuthorWebsite.Core.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static MHAuthorWebsite.GCommon.EntityConstraints.AnnouncementEmailDelivery;

namespace MHAuthorWebsite.Core.Models;

public class AnnouncementEmailDelivery
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Announcement))]
    public Guid AnnouncementId { get; set; }

    public Announcement Announcement { get; set; } = null!;

    [ForeignKey(nameof(User))]
    public string? UserId { get; set; }

    public ApplicationUser? User { get; set; }

    [Required]
    [MaxLength(EmailMaxLength)]
    public string Email { get; set; } = null!;

    [Required]
    public AnnouncementRecipientSource RecipientSource { get; set; }

    [Required]
    public AnnouncementDeliveryStatus DeliveryStatus { get; set; }

    [MaxLength(ErrorMessageMaxLength)]
    public string? ErrorMessage { get; set; }

    public DateTime? DeliveredOn { get; set; }
}
