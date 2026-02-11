namespace MHAuthorWebsite.Core.Dtos.Work;

public class WorkDetailsDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string CoverImageUrl { get; set; } = null!;

    public DateTime DatePublished { get; set; }

    public bool IsPublic { get; set; }
}
