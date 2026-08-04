namespace MHAuthorWebsite.Core.Dtos.Legal;

public class LegalDocumentNodeDto
{
    public int Number { get; set; }

    public string Title { get; set; } = null!;

    public string ContentDelta { get; set; } = null!;
}
