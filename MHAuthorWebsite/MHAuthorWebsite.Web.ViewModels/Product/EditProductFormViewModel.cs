using MHAuthorWebsite.Web.ViewModels.Localization;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using static MHAuthorWebsite.GCommon.EntityConstraints.Product;

namespace MHAuthorWebsite.Web.ViewModels.Product;

public class EditProductFormViewModel
{
    public Guid Id { get; set; }

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

    public string ProductTypeName { get; set; } = null!;

    public ICollection<IFormFile>? NewImages { get; set; } = new HashSet<IFormFile>();

    public ICollection<ProductImageViewModel> Images { get; set; } = new HashSet<ProductImageViewModel>();

    public string ImagesJson { get; set; } = null!;

    public ICollection<AttributeValueForm> Attributes { get; set; } = new HashSet<AttributeValueForm>();
}