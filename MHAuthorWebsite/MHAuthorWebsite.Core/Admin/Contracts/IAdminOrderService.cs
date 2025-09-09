using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Web.ViewModels.Admin.Order;

namespace MHAuthorWebsite.Core.Admin.Contracts;

public interface IAdminOrderService
{
    Task<ICollection<AllOrdersListItemViewModel>> GetAllOrders();

    Task<ServiceResult> AcceptOrderAsync(Guid orderId);

    Task<ServiceResult> RejectOrderAsync(Guid orderId);

    Task<ServiceResult> TerminateOrderAsync(Guid orderId);
}