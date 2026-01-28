namespace MHAuthorWebsite.Core.Dtos.Product;

public class ProductDetailsGeneralInfoDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string ProductTypeName { get; set; } = null!;

    public decimal Price { get; set; }

    public bool IsInStock { get; set; }

    public int Quantity { get; set; }

    public ProductDetailsDiscountDto? Discount { get; set; }

    public ICollection<ProductDetailsImageDto> Images { get; set; } = new HashSet<ProductDetailsImageDto>();

    public ICollection<ProductAttributeDetailsDto> Attributes { get; set; } = new HashSet<ProductAttributeDetailsDto>();
}