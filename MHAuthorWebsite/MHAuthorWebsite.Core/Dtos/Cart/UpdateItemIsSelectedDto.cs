namespace MHAuthorWebsite.Core.Dtos.Cart;

public class UpdateItemIsSelectedDto
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