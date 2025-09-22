using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Web.ViewModels.Order;
using Microsoft.AspNetCore.Mvc;

namespace MHAuthorWebsite.Web.Controllers;

public class OrderController : BaseController
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService) => _orderService = orderService;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        OrderSummaryViewModel model = await _orderService.GetOrderSummary(GetUserId()!);
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Order([FromBody] EcontDeliveryDetailsViewModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        ServiceResult result = await _orderService.Order(GetUserId()!, model);
        if (!result.Success) return StatusCode(500);

        return RedirectToAction(nameof(Index), "Cart");
    }

    [HttpGet]
    public async Task<IActionResult> MyOrders()
    {
        ICollection<MyOrdersViewModel> model = await _orderService.GetUserOrders(GetUserId()!);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> OrderDetails(Guid orderId)
    {
        ServiceResult<OrderDetailsViewModel> sr = await _orderService.GetOrderDetails(GetUserId()!, orderId);
        if (!sr.Success)
        {
            if (!sr.Found) return NotFound();
            if (!sr.HasPermission) return Forbid();

            return StatusCode(500);
        }

        return View(sr.Result);
    }
}