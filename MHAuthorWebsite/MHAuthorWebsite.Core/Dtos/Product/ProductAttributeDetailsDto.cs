using MHAuthorWebsite.Core.Models.Enums;

namespace MHAuthorWebsite.Core.Dtos.Product;

public class ProductAttributeDetailsDto
{
    public string Label { get; set; } = null!;

    public string? Value { get; set; } = null!;

    public AttributeDataType AttributeType { get; set; }

    public ProductAttributeDisplayPosition DisplayPosition { get; set; }
}