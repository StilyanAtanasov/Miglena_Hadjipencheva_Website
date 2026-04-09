using MHAuthorWebsite.Core.Admin.Dto;

namespace MHAuthorWebsite.Core.Dtos.Admin.Product;

public class AddProductDto
{
    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public int ProductTypeId { get; set; }

    public decimal Weight { get; set; }

    public int TitleImageId { get; set; } = 0;

    public ICollection<UploadImageRequestDto> Images { get; set; } = new HashSet<UploadImageRequestDto>();

    public ICollection<AttributeValueDto> Attributes { get; set; } = new HashSet<AttributeValueDto>();
}
