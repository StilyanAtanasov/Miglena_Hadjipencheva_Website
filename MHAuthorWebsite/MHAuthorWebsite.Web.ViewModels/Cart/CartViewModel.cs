namespace MHAuthorWebsite.Web.ViewModels.Cart;

public class CartViewModel
{
    public ICollection<CartItemViewModel> Items { get; set; } = new HashSet<CartItemViewModel>();

    public decimal Total => Items.Where(i => i is { IsAvailable: true, IsDiscontinued: false }).Sum(i => i.LineTotal);
}