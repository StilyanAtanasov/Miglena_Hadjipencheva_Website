using MHAuthorWebsite.Core.Dtos.Legal;
using MHAuthorWebsite.Core.Models.Enums;

namespace MHAuthorWebsite.Core.Dtos.Admin.LegalDocuments;

public class AdminLegalDocumentUpdateDto
{
    public LegalDocumentType DocumentType { get; set; }

    public string Title { get; set; } = null!;

    public ICollection<LegalDocumentNodeDto> Nodes { get; set; } = Array.Empty<LegalDocumentNodeDto>();
}
