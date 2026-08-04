namespace MHAuthorWebsite.Web.ViewModels.Admin.LegalDocuments;

public class LegalDocumentSummaryViewModel
{
    public string DocumentType { get; set; } = null!;

    public string Title { get; set; } = null!;

    public int Version { get; set; }

    public DateTime CreatedOn { get; set; }

    public string EditAction { get; set; } = null!;
}
