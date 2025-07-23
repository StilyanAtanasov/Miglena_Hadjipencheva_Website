namespace MHAuthorWebsite.Web.ViewModels.Product;

public class LikedProductViewModel
{
    public Guid Id { get; set; }

    public string? ThumbnailUrl { get; set; } = null!; // TODO remove nullability when not needed

    public string? ThumbnailAlt { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string ShortDescription { get; set; } = null!;

    public decimal Price { get; set; }

    public bool IsInStock { get; set; }
}