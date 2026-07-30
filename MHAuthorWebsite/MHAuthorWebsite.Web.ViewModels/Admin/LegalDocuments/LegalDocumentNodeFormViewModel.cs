using MHAuthorWebsite.Web.Common.Localization;
using System.ComponentModel.DataAnnotations;
using static MHAuthorWebsite.GCommon.EntityConstraints.LegalDocumentNode;

namespace MHAuthorWebsite.Web.ViewModels.Admin.LegalDocuments;

public class LegalDocumentNodeFormViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Номерът трябва да е положителен.")]
    public int Number { get; set; }

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Required")]
    [StringLength(TitleMaxLength, MinimumLength = TitleMinLength, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "StringLength")]
    public string Title { get; set; } = null!;

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Required")]
    [StringLength(ContentDeltaMaxLength, ErrorMessage = "Съдържанието е прекалено дълго.")]
    public string ContentDelta { get; set; } = null!;
}
