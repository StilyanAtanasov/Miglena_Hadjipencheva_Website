using MHAuthorWebsite.Core.Models;

namespace MHAuthorWebsite.Core.Contracts.DataServices;

public interface ICartDataService
{
    Task<Cart?> GetCartByUserIdReadonlyAsync(string userId);

    Task<Cart?> GetCartForItemQuantityUpdateAsync(string userId);

    Task<Cart?> GetCartForSelectionUpdateAsync(string userId);
}