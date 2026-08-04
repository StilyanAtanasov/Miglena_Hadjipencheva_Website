using MHAuthorWebsite.Core.Models.Enums;
using MHAuthorWebsite.Web.Common.Localization;
using System.ComponentModel.DataAnnotations;

namespace MHAuthorWebsite.Web.ViewModels.Admin.Product;

public class AttributeValueForm : IValidatableObject
{
    public int AttributeDefinitionId { get; set; }

    public string Key { get; set; } = null!;

    public string Label { get; set; } = null!;

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Required")]
    public ProductAttributeDisplayPosition DisplayPosition { get; set; }

    public AttributeDataType DataType { get; set; }

    public bool IsRequired { get; set; }

    public ICollection<AttributeOptionViewModel> PredefinedValues { get; set; } = new HashSet<AttributeOptionViewModel>();

    public string? Value { get; set; }

    public int? ProductAttributeOptionId { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (IsRequired && string.IsNullOrWhiteSpace(Value) && !ProductAttributeOptionId.HasValue)
            yield return new ValidationResult(
                $"Полето \"{Label}\" е задължително.",
                new[] { nameof(Value) }
            );
    }
}
