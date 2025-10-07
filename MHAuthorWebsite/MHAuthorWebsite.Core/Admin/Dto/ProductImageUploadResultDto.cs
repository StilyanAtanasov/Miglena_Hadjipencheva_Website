namespace MHAuthorWebsite.Core.Admin.Dto;

public class ProductImageUploadResultDto
{
    public string OriginalUrl { get; set; } = null!;

    public string? PreviewUrl { get; set; } = null!;

    public string PublicId { get; set; } = null!;

    public string? ThumbnailPublicId { get; set; } = null!;

    public bool IsThumbnail { get; set; }
}