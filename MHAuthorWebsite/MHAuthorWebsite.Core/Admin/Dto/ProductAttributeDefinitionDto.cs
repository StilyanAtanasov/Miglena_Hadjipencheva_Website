namespace MHAuthorWebsite.Core.Admin.Dto;

public class ProductAttributeDefinitionDto
{
    public int Id { get; set; }

    public string Label { get; set; } = null!;

    public bool IsRequired { get; set; }
}