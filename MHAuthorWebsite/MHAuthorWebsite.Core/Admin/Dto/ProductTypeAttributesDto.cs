namespace MHAuthorWebsite.Core.Admin.Dto;

public class ProductTypeAttributesDto
{
    public int AttributeDefinitionId { get; set; }

    public string Key { get; set; } = null!;

    public string Label { get; set; } = null!;

    public int DataType { get; set; }

    public bool HasPredefinedValue { get; set; }

    public bool IsRequired { get; set; }
}