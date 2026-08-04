using MHAuthorWebsite.Core.Models.Enums;

namespace MHAuthorWebsite.Core.Dtos.Admin.LegalDocuments;

public class AdminLegalDocumentSummaryDto
{
    public LegalDocumentType DocumentType { get; set; }

    public string Title { get; set; } = null!;

    public int Version { get; set; }

    public DateTime CreatedOn { get; set; }
}
