using MHAuthorWebsite.Core.Dtos.Order;
using MHAuthorWebsite.Core.Models;

namespace MHAuthorWebsite.Core.Contracts.DataServices;

public interface IOrderDataService
{
    Task<MyOrderDto[]> GetUserOrdersPagedReadonlyAsync(string userId, int page);

    Task<CartItem[]> GetOrderCartItemsByUserId(string userId);

    Task<Order?> GetOrderByIdForOrderDetails(Guid orderId);
}