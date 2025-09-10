using MHAuthorWebsite.Web.Common.Localization;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using static MHAuthorWebsite.GCommon.EntityConstraints.Product;

namespace MHAuthorWebsite.Web.ViewModels.Product;

public class AddProductForm
{
    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Required")]
    [StringLength(NameMaxLength, MinimumLength = NameMinLength, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "StringLength")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Required")]
    [StringLength(DescriptionDeltaMaxLength, MinimumLength = DescriptionDeltaMinLength, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "StringLength")]
    public string Description { get; set; } = null!;

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Required")]
    [Range(PriceMinValue, PriceMaxValue, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Range")]
    public decimal Price { get; set; }

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Required")]
    [Range(StockQuantityMinValue, StockQuantityMaxValue, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Range")]
    public int StockQuantity { get; set; }

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Required")]
    public int ProductTypeId { get; set; }

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Required")]
    [Range(WeightMinValue, WeightMaxValue, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Range")]
    public decimal Weight { get; set; }

    public int TitleImageId { get; set; } = 0; // Default is 0, meaning the first (if not only) image will have a thumbnail.

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Required")]
    public ICollection<IFormFile> Images { get; set; } = new HashSet<IFormFile>();

    public ICollection<AttributeValueForm> Attributes { get; set; } = new HashSet<AttributeValueForm>();
}