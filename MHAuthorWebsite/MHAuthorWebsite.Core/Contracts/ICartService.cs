using MHAuthorWebsite.Core.Common.Utils;

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
}