using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Dtos.Admin.Order;
using MHAuthorWebsite.Core.Models.Enums;
using MHAuthorWebsite.Data.Common.Extensions;
using MHAuthorWebsite.Data.Shared.Filters.Criteria;
using MHAuthorWebsite.Web.Utils.Attributes;
using MHAuthorWebsite.Web.Utils.Enums;
using MHAuthorWebsite.Web.ViewModels.Admin.Order;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MHAuthorWebsite.Web.Areas.Admin.Controllers;

public class AdminOrderController : AdminBaseController
{
    private readonly IAdminOrderService _adminOrderService;

    public AdminOrderController(IAdminOrderService adminOrderService) => _adminOrderService = adminOrderService;

    [SecurityHeaders(CspFeature.TomSelect)]
    [HttpGet]
    public async Task<IActionResult> AllOrders([FromQuery] AllOrdersFilterCriteria filter)
    {
        ICollection<AllOrdersListItemDto> orders = await _adminOrderService.GetAllOrders(filter);
        IEnumerable<OrderStatus?> statuses = Enum.GetValues<OrderStatus>().Cast<OrderStatus?>().Prepend(null);

        ViewData["StatusList"] = new SelectList(
            statuses.Select(s => new { Value = s?.ToString() ?? "All", Text = s?.GetDisplayName() ?? "Всички" }),
            "Value",
            "Text",
            filter.Status ?? "All"
        );

        ICollection<AllOrdersListItemViewModel> orderViewModels = orders
            .Select(o => new AllOrdersListItemViewModel
            {
                Id = o.Id,
                CustomerName = o.CustomerName,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                Currency = o.Currency,
                OrderDate = o.OrderDate,
            })
            .ToList();

        return View(orderViewModels);
    }

    [HttpGet]
    [SecurityHeaders(CspFeature.Econt)]
    public async Task<IActionResult> OrderDetails(Guid orderId)
    {
        ServiceResult<AdminOrderDetailsDto> sr = await _adminOrderService.GetOrderDetailsAsync(orderId);
        if (!sr.Found) return NotFound();

        AdminOrderDetailsDto dto = sr.Result!;
        AdminOrderDetailsViewModel viewModel = new()
        {
            OrderId = dto.OrderId,
            OrderDate = dto.OrderDate,
            Status = dto.Status,
            Products = dto.Products
                .Select(p => new AdminOrderProductDetailsViewModel
                {
                    Id = p.Id,
                    ProductName = p.ProductName,
                    Quantity = p.Quantity,
                    ImageUrl = p.ImageUrl,
                    UnitPrice = p.UnitPrice,
                })
                .ToList(),
            Shipment = new AdminOrderShipmentDetailsViewModel()
            {
                ShipmentNumber = dto.Shipment.ShipmentNumber,
                Currency = dto.Shipment.Currency,
                Address = dto.Shipment.Address,
                City = dto.Shipment.City,
                AwbUrl = dto.Shipment.AwbUrl,
                CourierName = dto.Shipment.CourierName,
                ExpectedDeliveryDate = dto.Shipment.ExpectedDeliveryDate,
                Email = dto.Shipment.Email,
                Face = dto.Shipment.Face,
                Phone = dto.Shipment.Phone,
                PostCode = dto.Shipment.PostCode,
                PriorityFrom = dto.Shipment.PriorityFrom,
                PriorityTo = dto.Shipment.PriorityTo,
                ShippingPrice = dto.Shipment.ShippingPrice,
                Services = dto.Shipment.Services
                    .Select(s => new AdminOrderShipmentServiceViewModel
                    {
                        Count = s.Count,
                        Currency = s.Currency,
                        Description = s.Description,
                        Price = s.Price,
                        PaymentSide = s.PaymentSide,
                        Type = s.Type,
                    })
                    .ToArray(),
                TrackingEvents = dto.Shipment.TrackingEvents
                    .Select(e => new AdminOrderShipmentEventViewModel
                    {
                        DestinationDetails = e.DestinationDetails,
                        CityName = e.CityName,
                        OfficeName = e.OfficeName,
                        Time = e.Time,
                    })
                    .ToArray(),
            }
        };

        return View(viewModel);
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