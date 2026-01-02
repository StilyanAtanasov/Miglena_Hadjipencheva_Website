using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Dtos.Order;

namespace MHAuthorWebsite.Core.Contracts;

public interface IOrderService
{
    Task<OrderSummaryDto> GetOrderSummary(string userId);

    Task<ServiceResult<Guid>> Order(string userId, EcontDeliveryDetailsDto model);

    Task<ICollection<MyOrderDto>> GetUserOrders(string userId, int page);

    Task<ServiceResult<OrderDetailsDto>> GetOrderDetails(string userId, Guid orderId);

    Task<bool> CanAccessSuccessPage(string userId, Guid orderId);
}