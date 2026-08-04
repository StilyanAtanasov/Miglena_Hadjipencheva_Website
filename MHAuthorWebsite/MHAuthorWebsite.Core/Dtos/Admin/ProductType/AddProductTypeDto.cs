namespace MHAuthorWebsite.Core.Dtos.Admin.ProductType;

public class AddProductTypeDto
{
    public string Name { get; set; } = null!;

    public bool HasAdditionalProperties { get; set; }

    public ICollection<AttributeDefinitionDto> Attributes { get; set; } = new HashSet<AttributeDefinitionDto>();
}
