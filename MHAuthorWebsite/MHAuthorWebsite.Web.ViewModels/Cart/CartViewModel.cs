namespace MHAuthorWebsite.Web.ViewModels.Cart;

public class CartViewModel
{
    public List<CartItemViewModel> Items { get; set; } = new();
    public decimal Total => Items.Where(i => i is { IsAvailable: true, IsDiscontinued: false }).Sum(i => i.LineTotal);
}