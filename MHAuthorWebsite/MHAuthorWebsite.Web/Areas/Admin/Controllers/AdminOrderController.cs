using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Web.ViewModels.Admin.Order;
using Microsoft.AspNetCore.Mvc;

namespace MHAuthorWebsite.Web.Areas.Admin.Controllers;

public class AdminOrderController : AdminBaseController
{
    private readonly IAdminOrderService _adminOrderService;

    public AdminOrderController(IAdminOrderService adminOrderService) => _adminOrderService = adminOrderService;

    [HttpGet]
    public async Task<IActionResult> AllOrders()
    {
        ICollection<AllOrdersListItemViewModel> orders = await _adminOrderService.GetAllOrders();
        return View(orders);
    }

    [HttpPost]
    public async Task<IActionResult> Accept(Guid orderId)
    {
        ServiceResult sr = await _adminOrderService.AcceptOrderAsync(orderId);

        if (sr is { Success: false, IsBadRequest: true }) return BadRequest();
        if (!sr.Success) return StatusCode(500);

        return RedirectToAction(nameof(AllOrders));
    }

    [HttpPost]
    public async Task<IActionResult> Reject(Guid orderId)
    {
        ServiceResult sr = await _adminOrderService.RejectOrderAsync(orderId);

        if (sr is { Success: false, IsBadRequest: true }) return BadRequest();
        if (!sr.Success) return StatusCode(500);

        return RedirectToAction(nameof(AllOrders));
    }
}