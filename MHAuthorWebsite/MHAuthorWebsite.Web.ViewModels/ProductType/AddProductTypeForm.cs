using System.ComponentModel.DataAnnotations;

namespace MHAuthorWebsite.Web.ViewModels.ProductType;

public class AddProductTypeForm
{
    [Required]
    public string Name { get; set; } = null!;
}
