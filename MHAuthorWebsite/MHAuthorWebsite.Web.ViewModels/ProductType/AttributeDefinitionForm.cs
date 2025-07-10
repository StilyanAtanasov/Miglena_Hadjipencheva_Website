using System.ComponentModel.DataAnnotations;
using static MHAuthorWebsite.GCommon.EntityConstraints.ProductAttributeDefinition;

namespace MHAuthorWebsite.Web.ViewModels.ProductType;

public class AttributeDefinitionForm
{
    [Required]
    [StringLength(KeyMaxLength, MinimumLength = KeyMinLength, ErrorMessage = "Ключът трябва да бъде между {2} и {1} символа!")]
    public string Key { get; set; } = null!;

    [Required]
    [StringLength(LabelMaxLength, MinimumLength = LabelMinLength, ErrorMessage = "Етикетът трябва да бъде между {2} и {1} символа!")]
    public string Label { get; set; } = null!;

    [Required]
    public int DataType { get; set; }

    [Required]
    public bool HasPredefinedValue { get; set; }

    [Required]
    public bool IsRequired { get; set; }
}