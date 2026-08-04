using MHAuthorWebsite.Core.Models.Enums;

namespace MHAuthorWebsite.Web.ViewModels.Product;

public class ProductAttributeDetailsViewModel
{
    public string Label { get; set; } = null!;

    public string? Value { get; set; } = null!;

    public AttributeDataType AttributeType { get; set; }

    public string? DisplayValue => FormatDisplayValue(Value, AttributeType);

    public ProductAttributeDisplayPosition DisplayPosition { get; set; }

    private static string? FormatDisplayValue(string? value, AttributeDataType type)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;

        if (type == AttributeDataType.Boolean)
        {
            string trimmed = value.Trim().ToLowerInvariant();

            if (bool.TryParse(trimmed, out bool boolResult))
                return boolResult ? "Да" : "Не";

            if (trimmed is "1" or "on" or "yes") return "Да";
            if (trimmed is "0" or "off" or "no") return "Не";
        }

        return value;
    }
}
