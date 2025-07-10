using MHAuthorWebsite.Web.ViewModels.Localization;
using System.ComponentModel.DataAnnotations;
using static MHAuthorWebsite.GCommon.EntityConstraints.ProductAttributeDefinition;

namespace MHAuthorWebsite.Web.ViewModels.ProductType;

public class AttributeDefinitionForm
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
    public bool HasPredefinedValue { get; set; }

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Required")]
    public bool IsRequired { get; set; }
}