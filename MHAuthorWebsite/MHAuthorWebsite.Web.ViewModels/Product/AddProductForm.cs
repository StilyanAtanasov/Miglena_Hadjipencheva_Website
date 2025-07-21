using MHAuthorWebsite.Web.ViewModels.Localization;
using System.ComponentModel.DataAnnotations;
using static MHAuthorWebsite.GCommon.EntityConstraints.Product;

namespace MHAuthorWebsite.Web.ViewModels.Product;

public class AddProductForm
{
    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Required")]
    [StringLength(NameMaxLength, MinimumLength = NameMinLength, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "StringLength")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Required")]
    [StringLength(DescriptionHtmlMaxLength, MinimumLength = DescriptionHtmlMinLength, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "StringLength")]
    public string Description { get; set; } = null!;

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Required")]
    [Range(PriceMinValue, PriceMaxValue, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Range")]
    public decimal Price { get; set; }

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Required")]
    [Range(StockQuantityMinValue, StockQuantityMaxValue, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Range")]
    public int StockQuantity { get; set; }

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Required")]
    public int ProductTypeId { get; set; }

    public ICollection<AttributeValueForm> Attributes { get; set; } = new HashSet<AttributeValueForm>();
}