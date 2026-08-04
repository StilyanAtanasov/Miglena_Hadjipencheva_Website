using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using static MHAuthorWebsite.GCommon.EntityConstraints.Work;

namespace MHAuthorWebsite.Web.ViewModels.Admin.Work;

public class AddWorkForm
{
    [Required(ErrorMessage = "Заглавието е задължително")]
    [MaxLength(TitleMaxLength, ErrorMessage = "Заглавието е прекалено дълго")]
    public string Title { get; set; } = null!;

    [Required(ErrorMessage = "Съдържанието е задължително")]
    public string Content { get; set; } = null!;

    [Required(ErrorMessage = "Корицата е задължителна")]
    public IFormFile CoverImage { get; set; } = null!;

    public bool IsPublic { get; set; } = true;
}
