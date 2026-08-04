using MHAuthorWebsite.Core.Models.Enums;

namespace MHAuthorWebsite.Core.Dtos.Legal;

public class LegalDocumentDto
{
    public Guid Id { get; set; }

    public LegalDocumentType DocumentType { get; set; }

    public int Version { get; set; }

    public string Title { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    public ICollection<LegalDocumentNodeDto> Nodes { get; set; } = Array.Empty<LegalDocumentNodeDto>();
}
