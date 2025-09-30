using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Data.Common.Extensions;
using MHAuthorWebsite.Data.Models.Enums;
using MHAuthorWebsite.Data.Shared.Filters.Criteria;
using MHAuthorWebsite.Web.ViewModels.Admin.Order;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MHAuthorWebsite.Web.Areas.Admin.Controllers;

public class AdminOrderController : AdminBaseController
{
    private readonly IAdminOrderService _adminOrderService;

    public AdminOrderController(IAdminOrderService adminOrderService) => _adminOrderService = adminOrderService;

    [HttpGet]
    public async Task<IActionResult> AllOrders([FromQuery] AllOrdersFilterCriteria filter)
    {
        ICollection<AllOrdersListItemViewModel> orders = await _adminOrderService.GetAllOrders(filter);
        IEnumerable<OrderStatus?> statuses = Enum.GetValues<OrderStatus>().Cast<OrderStatus?>().Prepend(null);

        ViewData["StatusList"] = new SelectList(
            statuses.Select(s => new { Value = s?.ToString() ?? "All", Text = s?.GetDisplayName() ?? "Всички" }),
            "Value",
            "Text",
            filter.Status ?? "All"
        );

        return View(orders);
    }

    [HttpGet]
    public async Task<IActionResult> OrderDetails(Guid orderId)
    {
        ServiceResult<AdminOrderDetailsViewModel> sr = await _adminOrderService.GetOrderDetailsAsync(orderId);
        if (!sr.Found) return NotFound();

        return View(sr.Result);
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

    [HttpPost]
    public async Task<IActionResult> Terminate(Guid orderId)
    {
        ServiceResult sr = await _adminOrderService.TerminateOrderAsync(orderId);

        if (sr is { Success: false, IsBadRequest: true }) return BadRequest();
        if (!sr.Success) return StatusCode(500);

        return RedirectToAction(nameof(AllOrders));
    }
}