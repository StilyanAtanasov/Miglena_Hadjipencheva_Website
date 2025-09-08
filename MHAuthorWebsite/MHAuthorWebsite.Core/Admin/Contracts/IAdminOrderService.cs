using MHAuthorWebsite.Web.ViewModels.Admin.Order;

namespace MHAuthorWebsite.Core.Admin.Contracts;

public interface IAdminOrderService
{
    Task<ICollection<AllOrdersListItemViewModel>> GetAllOrders();
}