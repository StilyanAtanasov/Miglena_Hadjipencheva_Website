namespace MHAuthorWebsite.Web.ViewModels.Product;

public class ProductImageViewModel
{
    public Guid Id { get; set; }

    public string Url { get; set; } = null!;

    public bool IsTitle { get; set; }
}