namespace MHAuthorWebsite.Core.Dtos.Product;

public class LikedProductDto
{
    public Guid Id { get; set; }

    public string ThumbnailUrl { get; set; } = null!;

    public string ThumbnailAlt { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string CategoryName { get; set; } = null!;

    public decimal Price { get; set; }

    public decimal? DiscountedPrice { get; set; }

    public bool IsInStock { get; set; }
}