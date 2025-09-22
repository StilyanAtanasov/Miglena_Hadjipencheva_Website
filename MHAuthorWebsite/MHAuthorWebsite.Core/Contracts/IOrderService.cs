using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Web.ViewModels.Order;

namespace MHAuthorWebsite.Core.Contracts;

public interface IOrderService
{
    Task<OrderDetailsViewModel> GetOrderDetails(string userId);

    Task<ServiceResult> Order(string userId, EcontDeliveryDetailsViewModel model);

    Task<ICollection<MyOrdersViewModel>> GetUserOrders(string userId);
}