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
        OrderDetailsViewModel model = await _orderService.GetOrderDetails(GetUserId()!);
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

    [HttpPost]
    public async Task<IActionResult> GetTrace(Guid orderId)
    {
        /*ServiceResult result = await _orderService.GetOrderTrackingInfo(GetUserId()!, orderId);
        if (!result.Success) return StatusCode(500);*/

        return Ok();
    }
}