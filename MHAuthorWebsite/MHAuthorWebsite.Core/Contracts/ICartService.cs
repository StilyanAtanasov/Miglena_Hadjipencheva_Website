using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Web.ViewModels.Cart;

namespace MHAuthorWebsite.Core.Contracts;

public interface ICartService
{
    /// <summary>
    /// Adds an item to the cart.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="productId">The ID of the product to add.</param>
    /// <param name="quantity">The quantity of the product to add.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<ServiceResult> AddItemToCartAsync(string userId, Guid productId, int quantity);

    /// <summary>
    /// Asynchronously retrieves the shopping cart for the specified user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose cart is to be retrieved. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of  <see
    /// cref="CartViewModel"/> objects representing the items in the user's cart. Returns an empty collection if the
    /// cart is empty.</returns>
    Task<CartViewModel> GetCartReadonlyAsync(string userId);

    /// <summary>
    /// Removes a specified CartItem from the user's shopping cart.
    /// </summary>
    /// <remarks>This method is asynchronous and should be awaited to ensure the operation completes before
    /// proceeding.</remarks>
    /// <param name="userId">The unique identifier of the user whose cart is being modified. Cannot be null or empty.</param>
    /// <param name="itemId">The unique identifier of the item to remove from the cart.</param>
    /// <returns>A <see cref="ServiceResult"/> indicating the success or failure of the operation.  The result contains additional
    /// details if the operation fails.</returns>
    Task<ServiceResult> RemoveFromCartAsync(string userId, Guid itemId);

    /// <summary>
    /// Updates the quantity of a specific item in the user's inventory.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose inventory is being updated. Cannot be null or empty.</param>
    /// <param name="itemId">The unique identifier of the item to update. Must be a valid <see cref="Guid"/>.</param>
    /// <param name="quantity">The new quantity to set for the specified item. Must be a non-negative integer.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="ServiceResult{T}"/> 
    /// with an <see cref="UpdatedItemQuantityViewModel"/> indicating the updated item details if the operation
    /// succeeds.</returns>
    Task<ServiceResult<UpdatedItemQuantityViewModel>> UpdateItemQuantityAsync(string userId, Guid itemId, int quantity);

    Task<ServiceResult> UpdateIsSelectedAsync(string userId, Guid itemId, bool isSelected);
}