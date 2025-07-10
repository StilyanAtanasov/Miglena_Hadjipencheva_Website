using System.ComponentModel.DataAnnotations;
using static MHAuthorWebsite.GCommon.EntityConstraints.ProductType;

namespace MHAuthorWebsite.Web.ViewModels.ProductType;

public class AddProductTypeForm
{
    [Required]
    [StringLength(NameMaxLength, MinimumLength = NameMinLength)]
    public string Name { get; set; } = null!;
}
