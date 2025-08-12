namespace MHAuthorWebsite.Web.ViewModels.Cart;

public class UpdateItemIsSelectedViewModel
{
    /// <summary>
    /// Gets or sets the unique identifier of the cart item.
    /// </summary>
    public Guid ItemId { get; set; }

    /// <summary>
    /// Gets or sets the new value for IsSelected.
    /// </summary>
    public bool IsSelected { get; set; }
}