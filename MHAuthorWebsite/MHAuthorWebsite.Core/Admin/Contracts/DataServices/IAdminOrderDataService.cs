using MHAuthorWebsite.Core.Models;

namespace MHAuthorWebsite.Core.Admin.Contracts.DataServices;

public interface IAdminOrderDataService
{
    Task<Order?> GetOrderByIdForOrderDetailsReadonlyAsync(Guid orderId);

    Task<Order?> GetOrderByIdForEditAndOrderDtoAsync(Guid orderId);

    Task<OrderProduct[]> GetOrderProductsByOrderByIdForRestoringProductAsync(Guid orderId);

}