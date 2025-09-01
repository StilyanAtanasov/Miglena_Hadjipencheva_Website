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
}