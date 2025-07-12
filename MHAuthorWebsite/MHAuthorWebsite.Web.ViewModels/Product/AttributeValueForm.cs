using MHAuthorWebsite.Data.Models.Enums;

namespace MHAuthorWebsite.Web.ViewModels.Product;

public class AttributeValueForm
{
    public string Key { get; set; } = null!;

    public string Label { get; set; } = null!;

    public AttributeDataType DataType { get; set; }

    public bool HasPredefinedValue { get; set; }

    public bool IsRequired { get; set; }
}