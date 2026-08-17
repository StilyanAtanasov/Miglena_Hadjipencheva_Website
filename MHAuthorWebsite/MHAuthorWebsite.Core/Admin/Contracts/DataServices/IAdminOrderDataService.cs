using MHAuthorWebsite.Core.Dtos.Admin.Order;
using MHAuthorWebsite.Core.Filters.Criteria;
using MHAuthorWebsite.Core.Models;

namespace MHAuthorWebsite.Core.Admin.Contracts.DataServices;

public interface IAdminOrderDataService
{
    Task<AllOrdersListItemDto[]> GetAllOrdersByFilterReadonlyAsync(AllOrdersFilterCriteria filter);

    Task<Order?> GetOrderByIdForOrderDetailsReadonlyAsync(Guid orderId);

    Task<Order?> GetOrderByIdForEditAndOrderDtoAsync(Guid orderId);

    Task<OrderProduct[]> GetOrderProductsByOrderByIdForRestoringProductAsync(Guid orderId);

}