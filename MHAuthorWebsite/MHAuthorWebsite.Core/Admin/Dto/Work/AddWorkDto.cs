using System.ComponentModel.DataAnnotations;
using static MHAuthorWebsite.GCommon.EntityConstraints.Work;

namespace MHAuthorWebsite.Core.Admin.Dto.Work;

public class AddWorkDto
{
    [Required]
    [MaxLength(TitleMaxLength)]
    public string Title { get; set; } = null!;

    [Required]
    public string Content { get; set; } = null!;

    [Required]
    public UploadImageRequestDto CoverImage { get; set; } = null!;

    public bool IsPublic { get; set; } = true;
}
