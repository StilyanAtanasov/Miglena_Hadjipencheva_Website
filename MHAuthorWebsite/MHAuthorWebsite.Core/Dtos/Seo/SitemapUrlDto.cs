namespace MHAuthorWebsite.Core.Dtos.Seo;

public class SitemapUrlDto
{
    public string RelativeUrl { get; init; } = null!;

    public DateTime? LastModifiedUtc { get; init; }

    public string? ChangeFrequency { get; init; }

    public decimal? Priority { get; init; }
}
