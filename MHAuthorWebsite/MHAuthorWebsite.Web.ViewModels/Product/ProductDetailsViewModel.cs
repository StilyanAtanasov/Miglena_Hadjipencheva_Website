namespace MHAuthorWebsite.Web.ViewModels.Product;

public class ProductDetailsViewModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string ProductTypeName { get; set; } = null!;

    public decimal Price { get; set; }

    public bool IsInStock { get; set; }

    public bool IsLiked { get; set; }

    public ICollection<ProductDetailsImage> Images { get; set; } = new HashSet<ProductDetailsImage>();

    public ICollection<ProductAttributeDetailsViewModel> Attributes { get; set; } = new HashSet<ProductAttributeDetailsViewModel>();

    public ICollection<ProductCommentViewModel> Comments { get; set; } = new HashSet<ProductCommentViewModel>();
}