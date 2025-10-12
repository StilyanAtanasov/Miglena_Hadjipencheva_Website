using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dto;
using MHAuthorWebsite.Data.Common.Extensions;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Data.Models.Enums;
using MHAuthorWebsite.Data.Shared;
using MHAuthorWebsite.Web.ViewModels.Order;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static MHAuthorWebsite.GCommon.ApplicationRules.Application;
using static MHAuthorWebsite.GCommon.ApplicationRules.OrderSystemEventsMessages;

namespace MHAuthorWebsite.Core;

public class OrderService : IOrderService
{
    protected readonly IApplicationRepository Repository;
    protected readonly UserManager<ApplicationUser> UserManager;
    protected readonly IEcontService EcontService;

    public OrderService(IApplicationRepository repository, UserManager<ApplicationUser> userManager, IEcontService econtService)
    {
        Repository = repository;
        UserManager = userManager;
        EcontService = econtService;
    }

    public async Task<OrderSummaryViewModel> GetOrderSummary(string userId)
    {
        ApplicationUser user = (await UserManager.FindByIdAsync(userId))!;

        ICollection<SelectedProductViewModel> selectedProducts = await Repository
            .WhereReadonly<CartItem>(ci => ci.Cart.UserId == userId && ci.IsSelected && ci.Product.IsPublic && ci.Product.StockQuantity >= ci.Quantity)
            .Include(ci => ci.Cart)
            .Include(ci => ci.Product)
                .ThenInclude(p => p.Thumbnail)
                    .ThenInclude(t => t.Image)
            .Select(ci => new SelectedProductViewModel
            {
                ImageUrl = ci.Product.Thumbnail.Image.ImageUrl,
                Name = ci.Product.Name,
                TotalPrice = ci.Product.Price * ci.Quantity,
                Quantity = ci.Quantity,
                TotalWeight = ci.Product.Weight * ci.Quantity
            })
            .ToListAsync();

        return new OrderSummaryViewModel
        {
            UserData = new()
            {
                Email = user.Email!,
                Name = user.Name!,
                PhoneNumber = user.PhoneNumber
            },
            SelectedProducts = selectedProducts
        };
    }

    public async Task<ServiceResult> Order(string userId, EcontDeliveryDetailsViewModel model)
    {
        CartItem[] cartItems = await Repository
            .Where<CartItem>(ci => ci.Cart.UserId == userId && ci.IsSelected && ci.Product.IsPublic && ci.Product.StockQuantity >= ci.Quantity)
            .Include(ci => ci.Cart)
            .Include(ci => ci.Product)
            .ToArrayAsync();

        EcontOrderDto orderDto = new()
        {
            Status = OrderStatus.InReview.GetDisplayName(),
            OrderTime = DateTime.UtcNow.Ticks,
            OrderSum = cartItems.Sum(ci => ci.Product.Price * ci.Quantity),
            Cod = true,
            PartialDelivery = false,
            Currency = Currency,
            CustomerInfo = new CustomerInfoDto
            {
                Id = model.Id,
                Name = model.Name,
                Face = model.Face,
                Phone = model.Phone,
                Email = model.Email,
                CityName = model.CityName,
                PostCode = model.PostCode,
                OfficeCode = model.OfficeCode,
                ZipCode = model.ZipCode,
                Address = model.Address,
                PriorityFrom = model.PriorityFrom,
                PriorityTo = model.PriorityTo,
                CountryCode = model.CountryCode
            },
            Items = cartItems
                .Select(i => new OrderItemDto
                {
                    Count = i.Quantity,
                    Name = i.Product.Name,
                    TotalPrice = i.Product.Price * i.Quantity,
                    TotalWeight = i.Product.Weight * i.Quantity
                }).ToArray()
        };

        ServiceResult<EcontOrderDto> sr = await EcontService.UpdateOrderAsync(orderDto);
        if (!sr.Success) return ServiceResult.Failure();

        EcontOrderDto createdOrder = sr.Result!;

        Order order = new()
        {
            Date = DateTime.UtcNow,
            UserId = userId,
            OrderedProducts = cartItems
               .Select(ci => new OrderProduct
               {
                   ProductId = ci.ProductId,
                   Quantity = ci.Quantity,
                   UnitPrice = ci.Product.Price,
               })
               .ToArray(),
            Shipment = new Shipment
            {
                CourierShipmentId = createdOrder.Id!.Value,
                ShippingPrice = model.ShippingPrice,
                OrderNumber = createdOrder.OrderNumber,
                Face = createdOrder.CustomerInfo.Face,
                Phone = createdOrder.CustomerInfo.Phone,
                Email = createdOrder.CustomerInfo.Email,
                City = createdOrder.CustomerInfo.CityName,
                PostCode = createdOrder.CustomerInfo.PostCode,
                Address = createdOrder.CustomerInfo.Address,
                PriorityFrom = createdOrder.CustomerInfo.PriorityFrom,
                PriorityTo = createdOrder.CustomerInfo.PriorityTo,
                Courier = Courier.Econt,
                Currency = Currency,
                Events = new HashSet<ShipmentEvent>
                {
                    new()
                    {
                        Time = DateTime.UtcNow,
                        Source = ShipmentEventSource.System,
                        DestinationDetails = AwaitingApproval
                    }
                }
            },
        };

        await Repository.AddAsync(order);

        foreach (CartItem item in cartItems) item.Product.StockQuantity -= item.Quantity;

        Repository.DeleteRange(cartItems);
        await Repository.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    public async Task<ICollection<MyOrdersViewModel>> GetUserOrders(string userId) =>
    await Repository
        .WhereReadonly<Order>(o => o.UserId == userId)
        .Include(o => o.OrderedProducts)
            .ThenInclude(op => op.Product)
                .ThenInclude(p => p.Thumbnail)
                    .ThenInclude(t => t.Image)
        .OrderByDescending(o => o.Date)
        .Select(o => new MyOrdersViewModel
        {
            OrderId = o.Id,
            CreatedAt = o.Date,
            Total = o.OrderedProducts.Sum(op => op.UnitPrice * op.Quantity) + o.Shipment.ShippingPrice,
            Status = o.Status.GetDisplayName(),
            Products = o.OrderedProducts
                .Select(op => new MyOrdersOrderProductViewModel
                {
                    ImageUrl = op.Product.Thumbnail.Image.ImageUrl,
                    Quantity = op.Quantity,
                })
                .ToArray()
        })
        .ToArrayAsync();

    public async Task<ServiceResult<OrderDetailsViewModel>> GetOrderDetails(string userId, Guid orderId)
    {
        Order? order = await Repository
            .WhereReadonly<Order>(o => o.Id == orderId)
            .Include(o => o.OrderedProducts)
                .ThenInclude(op => op.Product)
                    .ThenInclude(p => p.Thumbnail)
                        .ThenInclude(t => t.Image)
            .Include(o => o.Shipment)
                .ThenInclude(s => s.Events)
            .FirstOrDefaultAsync();

        if (order == null) return ServiceResult<OrderDetailsViewModel>.NotFound();
        if (order.UserId != userId) return ServiceResult<OrderDetailsViewModel>.Forbidden();

        OrderDetailsViewModel model = new()
        {
            OrderId = order.Id,
            OrderDate = order.Date,
            Status = order.Status.GetDisplayName(),
            Products = order.OrderedProducts
                 .Select(op => new OrderProductDetailsViewModel
                 {
                     ImageUrl = op.Product.Thumbnail.Image.ImageUrl,
                     ProductName = op.Product.Name,
                     UnitPrice = op.UnitPrice,
                     Quantity = op.Quantity,
                 })
                 .ToArray(),
            Shipment = new OrderShipmentDetailsViewModel
            {
                CourierName = order.Shipment.Courier.GetDisplayName(),
                ShipmentNumber = order.Shipment.ShipmentNumber,
                ShippingPrice = order.Shipment.ShippingPrice,
                Face = order.Shipment.Face,
                Phone = order.Shipment.Phone,
                Email = order.Shipment.Email,
                City = order.Shipment.City,
                PostCode = order.Shipment.PostCode,
                Address = order.Shipment.Address,
                PriorityFrom = order.Shipment.PriorityFrom,
                PriorityTo = order.Shipment.PriorityTo,
                TrackingEvents = order.Shipment.Events
                     .OrderBy(e => e.Time)
                     .Select(e => new OrderShipmentEventViewModel
                     {
                         CityName = e.CityName,
                         DestinationDetails = e.DestinationDetails!,
                         OfficeName = e.OfficeName,
                         Time = e.Time,
                     })
                     .ToArray(),
                ExpectedDeliveryDate = order.Shipment.ExpectedDeliveryDate,
                Currency = order.Shipment.Currency
            }
        };

        return ServiceResult<OrderDetailsViewModel>.Ok(model);
    }
}