using System.ComponentModel.DataAnnotations;
using static MHAuthorWebsite.GCommon.EntityConstraints.LegalDocumentNode;

namespace MHAuthorWebsite.Web.ViewModels.Admin.LegalDocuments;

public class LegalDocumentNodeFormViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Номерът трябва да е положителен.")]
    public int Number { get; set; }

    [Required]
    [StringLength(TitleMaxLength, MinimumLength = TitleMinLength)]
    public string Title { get; set; } = null!;

    [Required]
    [StringLength(ContentDeltaMaxLength)]
    public string ContentDelta { get; set; } = null!;
}
