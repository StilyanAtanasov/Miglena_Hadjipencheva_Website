namespace MHAuthorWebsite.Web.ViewModels.Product;

public class LikedProductViewModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string ShortDescription { get; set; } = null!;

    public decimal Price { get; set; }

    public bool IsInStock { get; set; }
}