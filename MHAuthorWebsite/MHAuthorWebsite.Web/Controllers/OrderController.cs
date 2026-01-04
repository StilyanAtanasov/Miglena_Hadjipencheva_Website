using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dtos.Order;
using MHAuthorWebsite.Web.Utils.Attributes;
using MHAuthorWebsite.Web.Utils.Enums;
using MHAuthorWebsite.Web.ViewModels.Order;
using Microsoft.AspNetCore.Mvc;

namespace MHAuthorWebsite.Web.Controllers;

public class OrderController : BaseController
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService) => _orderService = orderService;

    [SecurityHeaders(CspFeature.Notifications | CspFeature.Econt)]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        OrderSummaryDto orderDto = await _orderService.GetOrderSummary(GetUserId()!);
        if (orderDto.SelectedProducts.Count == 0) return RedirectToAction("Index", "Cart");

        OrderSummaryViewModel model = new OrderSummaryViewModel
        {
            SelectedProducts = orderDto.SelectedProducts
                .Select(p => new SelectedProductViewModel
                {
                    ImageUrl = p.ImageUrl,
                    Name = p.Name,
                    Quantity = p.Quantity,
                    TotalPrice = p.TotalPrice,
                    TotalWeight = p.TotalWeight,
                    TotalPriceWithDiscount = p.TotalPriceWithDiscount
                })
                .ToArray(),
            EcontShopId = orderDto.EcontShopId,
            UserData = new UserDataViewModel
            {
                Email = orderDto.UserData.Email,
                Name = orderDto.UserData.Name,
                PhoneNumber = orderDto.UserData.PhoneNumber,
            }
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Order([FromBody] EcontDeliveryDetailsViewModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        EcontDeliveryDetailsDto dto = new EcontDeliveryDetailsDto
        {
            Id = model.Id,
            Name = model.Name,
            Face = model.Face,
            Phone = model.Phone,
            Email = model.Email,
            CountryCode = model.CountryCode,
            CityName = model.CityName,
            PostCode = model.PostCode,
            OfficeCode = model.OfficeCode,
            ZipCode = model.ZipCode,
            Address = model.Address,
            PriorityFrom = model.PriorityFrom,
            PriorityTo = model.PriorityTo,
            ShippingPrice = model.ShippingPrice
        };

        ServiceResult<Guid> result = await _orderService.Order(GetUserId()!, dto);
        if (!result.Success) return StatusCode(500);

        return Ok(result.Result);
    }

    [HttpGet]
    public async Task<IActionResult> OrderAccepted(Guid orderId)
    {
        bool canAccess = await _orderService.CanAccessSuccessPage(GetUserId()!, orderId);
        if (!canAccess) return RedirectToAction(nameof(MyOrders));

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> MyOrders(int? page)
    {
        if (page is null || page < 1) page = 1;

        ICollection<MyOrderDto> dto = await _orderService.GetUserOrders(GetUserId()!, page.Value);

        ICollection<MyOrderViewModel> viewModels = dto
            .Select(o => new MyOrderViewModel
            {
                OrderId = o.OrderId,
                CreatedAt = o.CreatedAt,
                Total = o.Total,
                Status = o.Status,
                Products = o.Products
                        .Select(p => new MyOrdersOrderProductViewModel
                        {
                            Quantity = p.Quantity,
                            ImageUrl = p.ImageUrl,
                        })
                        .ToArray()
            })
            .ToArray();

        if (page == 1) return View(viewModels);
        return PartialView("_MyOrdersCards", viewModels);
    }

    [HttpGet]
    public async Task<IActionResult> OrderDetails(Guid orderId)
    {
        ServiceResult<OrderDetailsDto> sr = await _orderService.GetOrderDetails(GetUserId()!, orderId);
        if (!sr.Success)
        {
            if (!sr.Found) return NotFound();
            if (!sr.HasPermission) return Forbid();

            return StatusCode(500);
        }

        OrderDetailsDto dto = sr.Result!;

        OrderDetailsViewModel vm = new OrderDetailsViewModel
        {
            OrderId = dto.OrderId,
            Status = dto.Status,
            OrderDate = dto.OrderDate,
            Shipment = new OrderShipmentDetailsViewModel
            {
                Currency = dto.Shipment.Currency,
                Address = dto.Shipment.Address,
                City = dto.Shipment.City,
                PostCode = dto.Shipment.PostCode,
                ShippingPrice = dto.Shipment.ShippingPrice,
                CourierName = dto.Shipment.CourierName,
                Email = dto.Shipment.Email,
                ExpectedDeliveryDate = dto.Shipment.ExpectedDeliveryDate,
                Face = dto.Shipment.Face,
                Phone = dto.Shipment.Phone,
                PriorityFrom = dto.Shipment.PriorityFrom,
                PriorityTo = dto.Shipment.PriorityTo,
                ShipmentNumber = dto.Shipment.ShipmentNumber,
                TrackingEvents = dto.Shipment.TrackingEvents
                    .Select(e => new OrderShipmentEventViewModel
                    {
                        DestinationDetails = e.DestinationDetails,
                        CityName = e.CityName,
                        OfficeName = e.OfficeName,
                        Time = e.Time,
                    })
                    .ToArray()
            },
            Products = dto.Products
                .Select(p => new OrderProductDetailsViewModel
                {
                    ProductName = p.ProductName,
                    Quantity = p.Quantity,
                    UnitPrice = p.UnitPrice,
                    ImageUrl = p.ImageUrl
                })
                .ToArray()
        };

        return View(vm);
    }
}