namespace MHAuthorWebsite.Core.Dtos.Product;

public class LikedProductAllServiceDataDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public decimal? DiscountedPrice { get; set; }

    public DateTime? DiscountEnd { get; set; }

    public string CategoryName { get; set; } = null!;

    public bool IsInStock { get; set; }

    public string ThumbnailUrl { get; set; } = null!;

    public string ThumbnailAlt { get; set; } = null!;
}