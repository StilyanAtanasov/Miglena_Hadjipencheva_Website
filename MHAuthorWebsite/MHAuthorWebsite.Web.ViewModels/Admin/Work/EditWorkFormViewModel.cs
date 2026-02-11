using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using static MHAuthorWebsite.GCommon.EntityConstraints.Work;

namespace MHAuthorWebsite.Web.ViewModels.Admin.Work;

public class EditWorkFormViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Заглавието е задължително")]
    [MaxLength(TitleMaxLength, ErrorMessage = "Заглавието е прекалено дълго")]
    public string Title { get; set; } = null!;

    [Required(ErrorMessage = "Съдържанието е задължително")]
    public string Content { get; set; } = null!;

    public string? CurrentCoverImageUrl { get; set; }
    
    public IFormFile? NewCoverImage { get; set; }

    public bool IsPublic { get; set; }
}
