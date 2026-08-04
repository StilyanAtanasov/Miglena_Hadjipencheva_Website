namespace MHAuthorWebsite.Core.Dtos.Product;

public class ProductCardServiceDataDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = null!;

    public decimal Price { get; init; }

    public bool IsAvailable { get; init; }

    public string ProductType { get; init; } = null!;

    public string ImageUrl { get; init; } = null!;

    public string ImageAlt { get; init; } = null!;

    public decimal? DiscountPrice { get; init; }

    public DateTime? DiscountEnd { get; init; }
}