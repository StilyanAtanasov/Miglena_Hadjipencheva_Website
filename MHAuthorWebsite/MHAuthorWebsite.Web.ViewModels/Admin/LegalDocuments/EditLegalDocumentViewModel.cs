using MHAuthorWebsite.Core.Models.Enums;
using System.ComponentModel.DataAnnotations;
using static MHAuthorWebsite.GCommon.EntityConstraints.LegalDocument;

namespace MHAuthorWebsite.Web.ViewModels.Admin.LegalDocuments;

public class EditLegalDocumentViewModel
{
    public LegalDocumentType DocumentType { get; set; }

    public string DocumentTypeDisplayName { get; set; } = null!;

    public int CurrentVersion { get; set; }

    [Required]
    [StringLength(TitleMaxLength, MinimumLength = TitleMinLength)]
    public string Title { get; set; } = null!;

    public List<LegalDocumentNodeFormViewModel> Nodes { get; set; } = new();
}
