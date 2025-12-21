using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Dtos.Admin.Order;
using MHAuthorWebsite.Data.Shared.Filters.Criteria;

namespace MHAuthorWebsite.Core.Admin.Contracts;

public interface IAdminOrderService
{
    Task<ICollection<AllOrdersListItemDto>> GetAllOrders(AllOrdersFilterCriteria filter);

    Task<ServiceResult<AdminOrderDetailsDto>> GetOrderDetailsAsync(Guid orderId);

    Task<ServiceResult> AcceptOrderAsync(Guid orderId);

    Task<ServiceResult> RejectOrderAsync(Guid orderId);

    Task<ServiceResult> TerminateOrderAsync(Guid orderId);
}