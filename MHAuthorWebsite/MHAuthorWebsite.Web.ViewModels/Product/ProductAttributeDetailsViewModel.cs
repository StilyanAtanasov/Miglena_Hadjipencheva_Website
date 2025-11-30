using MHAuthorWebsite.Data.Models.Enums;

namespace MHAuthorWebsite.Web.ViewModels.Product;

public class ProductAttributeDetailsViewModel
{
    public string Label { get; set; } = null!;

    public string? Value { get; set; } = null!;

    public ProductAttributeDisplayPosition DisplayPosition { get; set; }
}