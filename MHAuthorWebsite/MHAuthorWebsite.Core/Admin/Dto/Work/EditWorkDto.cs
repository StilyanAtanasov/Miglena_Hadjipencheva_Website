using MHAuthorWebsite.Core.Dtos.Images;
using System.ComponentModel.DataAnnotations;
using static MHAuthorWebsite.GCommon.EntityConstraints.Work;

namespace MHAuthorWebsite.Core.Admin.Dto.Work;

public class EditWorkDto
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(TitleMaxLength)]
    public string Title { get; set; } = null!;

    [Required]
    public string Content { get; set; } = null!;

    public string? CurrentCoverImageUrl { get; set; }
    
    public UploadImageRequestDto? NewCoverImage { get; set; }

    public bool IsPublic { get; set; }
}
