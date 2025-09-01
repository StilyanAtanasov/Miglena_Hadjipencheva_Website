using MHAuthorWebsite.Web.ViewModels.Order;

namespace MHAuthorWebsite.Core.Contracts;

public interface IOrderService
{
    Task<OrderDetailsViewModel> GetOrderDetails(string userId);
}