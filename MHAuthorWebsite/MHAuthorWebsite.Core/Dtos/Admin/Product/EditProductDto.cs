using MHAuthorWebsite.Core.Dtos.Product;
using Microsoft.AspNetCore.Http;

namespace MHAuthorWebsite.Core.Dtos.Admin.Product;

public class EditProductDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public decimal Weight { get; set; }

    public string ProductTypeName { get; set; } = null!;

    public ICollection<IFormFile>? NewImages { get; set; } = new HashSet<IFormFile>();

    public ICollection<ProductImageDto> Images { get; set; } = new HashSet<ProductImageDto>();

    public string ImagesJson { get; set; } = null!;

    public ICollection<AttributeValueDto> Attributes { get; set; } = new HashSet<AttributeValueDto>();
}