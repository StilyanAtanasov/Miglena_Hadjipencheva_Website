using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Data.Shared.Filters.Criteria;
using MHAuthorWebsite.Web.ViewModels.Admin.Order;

namespace MHAuthorWebsite.Core.Admin.Contracts;

public interface IAdminOrderService
{
    Task<ICollection<AllOrdersListItemViewModel>> GetAllOrders(AllOrdersFilterCriteria filter);

    Task<ServiceResult<AdminOrderDetailsViewModel>> GetOrderDetailsAsync(Guid orderId);

    Task<ServiceResult> AcceptOrderAsync(Guid orderId);

    Task<ServiceResult> RejectOrderAsync(Guid orderId);

    Task<ServiceResult> TerminateOrderAsync(Guid orderId);
}