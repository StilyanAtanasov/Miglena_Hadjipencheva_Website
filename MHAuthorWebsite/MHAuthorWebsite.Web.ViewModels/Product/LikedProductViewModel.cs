namespace MHAuthorWebsite.Web.ViewModels.Product;

public class LikedProductViewModel
{
    public Guid Id { get; set; }

    public string ThumbnailUrl { get; set; } = null!;

    public string ThumbnailAlt { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string CategoryName { get; set; } = null!;

    public decimal Price { get; set; }

    public bool IsInStock { get; set; }
}