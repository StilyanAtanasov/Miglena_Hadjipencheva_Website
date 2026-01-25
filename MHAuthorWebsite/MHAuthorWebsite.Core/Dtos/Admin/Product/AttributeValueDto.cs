using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Models.Enums;

namespace MHAuthorWebsite.Core.Dtos.Admin.Product;

public class AttributeValueDto
{
    public int AttributeDefinitionId { get; set; }

    public string Key { get; set; } = null!;

    public string Label { get; set; } = null!;

    public ProductAttributeDisplayPosition DisplayPosition { get; set; }

    public AttributeDataType DataType { get; set; }

    public bool IsRequired { get; set; }

    public string? Value { get; set; }

    public int? ProductAttributeOptionId { get; set; }

    public ICollection<AttributeOptionDto> PredefinedValues { get; set; } = null!;
}
