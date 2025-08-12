namespace MHAuthorWebsite.Web.ViewModels.Cart;

public class CartItemViewModel
{
    public Guid ItemId { get; set; }

    public Guid ProductId { get; set; }

    public bool IsSelected { get; set; }

    public string ThumbnailUrl { get; set; } = null!;

    public string ThumbnailAlt { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Category { get; set; } = null!;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public bool IsDiscontinued { get; set; }

    public bool IsAvailable { get; set; }

    public decimal LineTotal => UnitPrice * Quantity;
}