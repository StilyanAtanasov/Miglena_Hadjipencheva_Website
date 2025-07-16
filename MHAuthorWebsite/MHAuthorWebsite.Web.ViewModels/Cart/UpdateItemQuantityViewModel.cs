namespace MHAuthorWebsite.Web.ViewModels.Cart;

public class UpdateItemQuantityViewModel
{
    /// <summary>
    /// Gets or sets the unique identifier of the cart item.
    /// </summary>
    public Guid ItemId { get; set; }

    /// <summary>
    /// Gets or sets the new quantity for the cart item.
    /// </summary>
    public int Quantity { get; set; }
}