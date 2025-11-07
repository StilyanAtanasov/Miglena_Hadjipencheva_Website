using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static MHAuthorWebsite.GCommon.EntityConstraints.ContactRequest;

namespace MHAuthorWebsite.Data.Models;

public class ContactRequest
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(NameMaxLength)]
    public string Name { get; set; } = null!;

    [Required]
    [MaxLength(EmailMaxLength)]
    public string Email { get; set; } = null!;

    [Required]
    [MaxLength(SubjectMaxLength)]
    public string Subject { get; set; } = null!;

    [Required]
    [MaxLength(MessageMaxLength)]
    public string Message { get; set; } = null!;

    [Required]
    public DateTime CreatedOn { get; set; }

    public DateTime? RepliedOn { get; set; }

    [MaxLength(ReplyMessageMaxLength)]
    public string? ReplyMessage { get; set; }

    [ForeignKey(nameof(Admin))]
    public string? AdminId { get; set; }

    public ApplicationUser? Admin { get; set; }

    [ForeignKey(nameof(User))]
    public string? UserId { get; set; } = null!;

    public ApplicationUser? User { get; set; } = null!;
}