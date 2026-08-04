using MHAuthorWebsite.Web.Common.Localization;
using System.ComponentModel.DataAnnotations;
using static MHAuthorWebsite.GCommon.EntityConstraints.ProductAttributeDefinition;

namespace MHAuthorWebsite.Web.ViewModels.Admin.ProductType;

public class AttributeDefinitionForm : IValidatableObject
{
    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Required")]
    [StringLength(KeyMaxLength, MinimumLength = KeyMinLength, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "StringLength")]
    public string Key { get; set; } = null!;

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Required")]
    [StringLength(LabelMaxLength, MinimumLength = LabelMinLength, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "StringLength")]
    public string Label { get; set; } = null!;

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Required")]
    public int DataType { get; set; }

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Required")]
    public bool IsRequired { get; set; }

    public ICollection<string> PredefinedValues { get; set; } = new HashSet<string>();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DataType == (int)Core.Models.Enums.AttributeDataType.Dropdown && PredefinedValues.Count < 2)
        {
            yield return new ValidationResult(
                errorMessage: "Необходими са поне 2 предварително зададени стойности, когато типът на данните е падащ списък.",
                memberNames: new[] { nameof(DataType) });
        }
    }
}