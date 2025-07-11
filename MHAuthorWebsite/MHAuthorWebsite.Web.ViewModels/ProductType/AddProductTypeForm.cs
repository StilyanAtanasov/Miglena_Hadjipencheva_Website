using MHAuthorWebsite.Web.ViewModels.Localization;
using System.ComponentModel.DataAnnotations;
using static MHAuthorWebsite.GCommon.EntityConstraints.ProductType;

namespace MHAuthorWebsite.Web.ViewModels.ProductType;

public class AddProductTypeForm
{
    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Required")]
    [StringLength(NameMaxLength, MinimumLength = NameMinLength, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "StringLength")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Required")]
    public bool HasAdditionalProperties { get; set; }

    public List<AttributeDefinitionForm> Attributes { get; set; } = new List<AttributeDefinitionForm>();
}
