using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Configuration.EcontApi;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Contracts.DataServices;
using MHAuthorWebsite.Core.Dtos.Order;
using MHAuthorWebsite.Core.Extensions;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using MHAuthorWebsite.Core.Models.Enums;
using MHAuthorWebsite.Data.Common.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using static MHAuthorWebsite.GCommon.ApplicationRules.Application;
using static MHAuthorWebsite.GCommon.ApplicationRules.Order;
using static MHAuthorWebsite.GCommon.ApplicationRules.OrderSystemEventsMessages;

namespace MHAuthorWebsite.Core;

public class OrderService : IOrderService
{
    protected readonly IApplicationRepository Repository;
    protected readonly IOrderDataService OrderDataService;
    protected readonly UserManager<ApplicationUser> UserManager;
    protected readonly IEcontService EcontService;
    protected readonly EcontApiSettings EcontApiSettings;

    public OrderService(IApplicationRepository repository, IOrderDataService orderDataService, UserManager<ApplicationUser> userManager,
        IEcontService econtService, IOptions<EcontApiSettings> econtApiSettings)
    {
        Repository = repository;
        UserManager = userManager;
        EcontService = econtService;
        EcontApiSettings = econtApiSettings.Value;
        OrderDataService = orderDataService;
    }

    public async Task<OrderSummaryDto> GetOrderSummary(string userId)
    {
        ApplicationUser user = (await UserManager.FindByIdAsync(userId))!;

        ICollection<SelectedProductDto> selectedProducts = await Repository
            .WhereReadonly<CartItem>(ci => ci.Cart.UserId == userId && ci.IsSelected && ci.Product.IsPublic && ci.Product.StockQuantity >= ci.Quantity)
            .Select(ci => new SelectedProductDto
            {
                ImageUrl = ci.Product.Thumbnail.Image.ImageUrl,
                Name = ci.Product.Name,
                TotalPrice = ci.Product.Price * ci.Quantity,
                TotalPriceWithDiscount = ci.Product.Discounts.FirstOrDefault(d => d.StartDate <= DateTime.UtcNow && d.EndDate >= DateTime.UtcNow) != null
                    ? ci.Product.Discounts.First(d => d.StartDate <= DateTime.UtcNow && d.EndDate >= DateTime.UtcNow).NewPrice * ci.Quantity
                    : null,
                Quantity = ci.Quantity,
                TotalWeight = ci.Product.Weight * ci.Quantity
            })
            .ToListAsync();

        return new OrderSummaryDto
        {
            UserData = new()
            {
                Email = user.Email!,
                Name = user.Name!,
                PhoneNumber = user.PhoneNumber
            },
            SelectedProducts = selectedProducts,
            EcontShopId = EcontApiSettings.EcontApiShopId
        };
    }

    public async Task<ServiceResult<Guid>> Order(string userId, EcontDeliveryDetailsDto model)
    {
        CartItem[] cartItems = await OrderDataService.GetOrderCartItemsByUserId(userId);

        Dictionary<Guid, decimal> productPricesWithDiscounts = cartItems.ToDictionary(
            ci => ci.ProductId,
            ci => ci.Product.Discounts.FirstOrDefault(d => d.StartDate <= DateTime.UtcNow && d.EndDate >= DateTime.UtcNow)?.NewPrice ?? ci.Product.Price);

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
                    TotalPrice = productPricesWithDiscounts[i.ProductId] * i.Quantity,
                    TotalWeight = i.Product.Weight * i.Quantity
                }).ToArray()
        };

        ServiceResult<EcontOrderDto> sr = await EcontService.UpdateOrderAsync(orderDto);
        if (!sr.Success) return ServiceResult<Guid>.Failure();

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
                   UnitPrice = productPricesWithDiscounts[ci.ProductId],
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
            }
        };

        await Repository.AddAsync(order);

        foreach (CartItem item in cartItems) item.Product.StockQuantity -= item.Quantity;

        Repository.DeleteRange(cartItems);
        await Repository.SaveChangesAsync();

        return ServiceResult<Guid>.Ok(order.Id);
    }

    public async Task<ICollection<MyOrderDto>> GetUserOrders(string userId, int page) =>
    await Repository
        .WhereReadonly<Order>(o => o.UserId == userId)
        .OrderByDescending(o => o.Date)
        .Skip((page - 1) * MyOrdersPageSize)
        .Take(MyOrdersPageSize)
        .Select(o => new MyOrderDto
        {
            OrderId = o.Id,
            CreatedAt = o.Date,
            Total = o.OrderedProducts.Sum(op => op.UnitPrice * op.Quantity) + o.Shipment.ShippingPrice,
            Status = o.Status.GetDisplayName(),
            Products = o.OrderedProducts
                .Select(op => new MyOrdersOrderProductDto
                {
                    ImageUrl = op.Product.Thumbnail.Image.ImageUrl,
                    Quantity = op.Quantity,
                })
                .ToArray()
        })
        .ToArrayAsync();

    public async Task<ServiceResult<OrderDetailsDto>> GetOrderDetails(string userId, Guid orderId)
    {
        Order? order = await OrderDataService.GetOrderByIdForOrderDetails(orderId);

        if (order == null) return ServiceResult<OrderDetailsDto>.NotFound();
        if (order.UserId != userId) return ServiceResult<OrderDetailsDto>.Forbidden();

        OrderDetailsDto model = new()
        {
            OrderId = order.Id,
            OrderDate = order.Date,
            Status = order.Status.GetDisplayName(),
            Products = order.OrderedProducts
                 .Select(op => new OrderProductDetailsDto
                 {
                     ImageUrl = op.Product.Thumbnail.Image.ImageUrl,
                     ProductName = op.Product.Name,
                     UnitPrice = op.UnitPrice,
                     Quantity = op.Quantity,
                 })
                 .ToArray(),
            Shipment = new OrderShipmentDetailsDto
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
                     .Select(e => new OrderShipmentEventDto
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

        return ServiceResult<OrderDetailsDto>.Ok(model);
    }

    public async Task<bool> CanAccessSuccessPage(string userId, Guid orderId)
        => await Repository
            .WhereReadonly<Order>(o => o.Id == orderId && o.UserId == userId && o.Date > DateTime.UtcNow.AddSeconds(-SuccessPageMaxViewDelaySeconds))
            .AnyAsync();
}