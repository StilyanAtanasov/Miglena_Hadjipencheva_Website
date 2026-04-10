namespace MHAuthorWebsite.Core.Dtos.Admin.ProductType;

public class AttributeDefinitionDto
{
    public string Key { get; set; } = null!;

    public string Label { get; set; } = null!;

    public int DataType { get; set; }

    public bool IsRequired { get; set; }

    public ICollection<string> PredefinedValues { get; set; } = new HashSet<string>();
}