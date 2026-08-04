namespace MHAuthorWebsite.Core.Dtos.Work;

public class WorkCardDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string CoverImageUrl { get; set; } = null!;
    public bool IsPublic { get; set; }
}
