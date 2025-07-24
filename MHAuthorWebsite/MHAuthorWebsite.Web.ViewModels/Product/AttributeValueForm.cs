using MHAuthorWebsite.Data.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace MHAuthorWebsite.Web.ViewModels.Product;

public class AttributeValueForm : IValidatableObject
{
    public int AttributeDefinitionId { get; set; }

    public string Key { get; set; } = null!;

    public string Label { get; set; } = null!;

    public AttributeDataType DataType { get; set; }

    public bool IsRequired { get; set; }

    public bool HasPredefinedValue { get; set; }

    public string? Value { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (IsRequired && string.IsNullOrWhiteSpace(Value))
        {
            yield return new ValidationResult(
                $"Полето \"{Label ?? Key}\" е задължително.",
                new[] { nameof(Value) }
            );
        }
    }
}
