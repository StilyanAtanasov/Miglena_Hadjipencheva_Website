using Microsoft.AspNetCore.Http;

namespace MHAuthorWebsite.Core.Dtos.Admin.Product;

public class AddProductDto
{
    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public int ProductTypeId { get; set; }

    public decimal Weight { get; set; }

    public int TitleImageId { get; set; } = 0; // Default is 0, meaning the first (if not only) image will have a thumbnail.

    public ICollection<IFormFile> Images { get; set; } = new HashSet<IFormFile>();

    public ICollection<AttributeValueDto> Attributes { get; set; } = new HashSet<AttributeValueDto>();
}