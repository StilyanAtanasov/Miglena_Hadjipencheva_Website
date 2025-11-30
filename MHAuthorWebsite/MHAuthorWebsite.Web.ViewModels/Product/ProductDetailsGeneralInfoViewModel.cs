namespace MHAuthorWebsite.Web.ViewModels.Product;

public class ProductDetailsGeneralInfoViewModel
{
    public Guid Id { get; set; }

    public Guid CacheCommitId { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string ProductTypeName { get; set; } = null!;

    public decimal Price { get; set; }

    public bool IsInStock { get; set; }

    public ProductDetailsDiscountViewModel? Discount { get; set; }

    public ICollection<ProductDetailsImageViewModel> Images { get; set; } = new HashSet<ProductDetailsImageViewModel>();

    public ICollection<ProductAttributeDetailsViewModel> Attributes { get; set; } = new HashSet<ProductAttributeDetailsViewModel>();
}