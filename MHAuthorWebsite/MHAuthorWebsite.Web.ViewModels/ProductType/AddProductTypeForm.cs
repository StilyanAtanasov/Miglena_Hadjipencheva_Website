using System.ComponentModel.DataAnnotations;
using static MHAuthorWebsite.GCommon.EntityConstraints.ProductType;

namespace MHAuthorWebsite.Web.ViewModels.ProductType;

public class AddProductTypeForm
{
    [Required]
    [StringLength(NameMaxLength, MinimumLength = NameMinLength, ErrorMessage = "Името трябва да бъде между {2} и {1} символа!")]
    public string Name { get; set; } = null!;
}
