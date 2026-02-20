using MHAuthorWebsite.Core.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static MHAuthorWebsite.GCommon.EntityConstraints.LegalDocument;

namespace MHAuthorWebsite.Core.Models;

public class LegalDocument
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public LegalDocumentType DocumentType { get; set; }

    [Required]
    public int Version { get; set; }

    [Required]
    [MaxLength(TitleMaxLength)]
    public string Title { get; set; } = null!;

    [Required]
    [MaxLength(NodesJsonMaxLength)]
    public string NodesJson { get; set; } = null!;

    [Required]
    public DateTime CreatedOn { get; set; }

    [ForeignKey(nameof(Admin))]
    public string? AdminId { get; set; }

    public ApplicationUser? Admin { get; set; }

    public ICollection<UserLegalAgreement> UserAgreements { get; set; } = new HashSet<UserLegalAgreement>();
}
