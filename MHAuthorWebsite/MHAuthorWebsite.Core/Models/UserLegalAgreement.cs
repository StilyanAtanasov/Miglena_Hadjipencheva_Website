using MHAuthorWebsite.Core.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MHAuthorWebsite.Core.Models;

public class UserLegalAgreement
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = null!;

    public ApplicationUser User { get; set; } = null!;

    [Required]
    [ForeignKey(nameof(Document))]
    public Guid DocumentId { get; set; }

    public LegalDocument Document { get; set; } = null!;

    [Required]
    public LegalDocumentType DocumentType { get; set; }

    [Required]
    public int DocumentVersion { get; set; }

    [Required]
    public DateTime AgreedOn { get; set; }
}
