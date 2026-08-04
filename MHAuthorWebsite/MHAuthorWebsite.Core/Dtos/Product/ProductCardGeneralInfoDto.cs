namespace MHAuthorWebsite.Core.Dtos.Product;

public class ProductCardGeneralInfoDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string ProductType { get; set; } = null!;

    public decimal Price { get; set; }

    public decimal? DiscountPrice { get; set; }

    public bool IsAvailable { get; set; }

    public string ImageUrl { get; set; } = null!;

    public string ImageAlt { get; set; } = null!;
}