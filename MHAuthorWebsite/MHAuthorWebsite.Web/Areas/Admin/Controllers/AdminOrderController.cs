using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Web.ViewModels.Admin.Order;
using Microsoft.AspNetCore.Mvc;

namespace MHAuthorWebsite.Web.Areas.Admin.Controllers;

public class AdminOrderController : AdminBaseController
{
    private readonly IAdminOrderService _adminOrderService;

    public AdminOrderController(IAdminOrderService adminOrderService) => _adminOrderService = adminOrderService;

    public async Task<IActionResult> AllOrders()
    {
        ICollection<AllOrdersListItemViewModel> orders = await _adminOrderService.GetAllOrders();
        return View(orders);
    }
}