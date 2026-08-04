using MHAuthorWebsite.Core.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static MHAuthorWebsite.GCommon.EntityConstraints.Announcement;

namespace MHAuthorWebsite.Core.Models;

public class Announcement
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(SubjectMaxLength)]
    public string Subject { get; set; } = null!;

    [Required]
    public string MessageDelta { get; set; } = null!;

    [Required]
    public AnnouncementRecipientGroup RecipientGroup { get; set; }

    [MaxLength(AdditionalRecipientsMaxLength)]
    public string? AdditionalRecipients { get; set; }

    [Required]
    public int RecipientCount { get; set; }

    [Required]
    public DateTime CreatedOn { get; set; }

    [ForeignKey(nameof(Admin))]
    public string AdminId { get; set; } = null!;

    public ApplicationUser Admin { get; set; } = null!;

    public ICollection<AnnouncementEmailDelivery> EmailDeliveries { get; set; } = new HashSet<AnnouncementEmailDelivery>();
}
