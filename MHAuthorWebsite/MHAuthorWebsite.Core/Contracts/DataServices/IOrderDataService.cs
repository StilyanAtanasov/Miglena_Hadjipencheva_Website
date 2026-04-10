using MHAuthorWebsite.Core.Models;

namespace MHAuthorWebsite.Core.Contracts.DataServices;

public interface IOrderDataService
{
    Task<CartItem[]> GetOrderCartItemsByUserId(string userId);

    Task<Order?> GetOrderByIdForOrderDetails(Guid orderId);
}