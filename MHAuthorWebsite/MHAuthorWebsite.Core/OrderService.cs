using MHAuthorWebsite.Core.Common.Extensions;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Configuration.EcontApi;
using MHAuthorWebsite.Core.Configuration.EmailConfiguration.Contracts;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Contracts.DataServices;
using MHAuthorWebsite.Core.Dtos.Order;
using MHAuthorWebsite.Core.Extensions;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using MHAuthorWebsite.Core.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static MHAuthorWebsite.GCommon.ApplicationRules.Application;
using static MHAuthorWebsite.GCommon.ApplicationRules.CacheKeys;
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
    protected readonly IEmailService EmailService;
    protected readonly IEmailUserProvider EmailUserProvider;
    protected readonly IAdminNotificationPreferencesService AdminNotificationPreferencesService;
    protected readonly IUrlProvider UrlProvider;
    protected readonly IFastCacheService CacheService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IApplicationRepository repository, IOrderDataService orderDataService, UserManager<ApplicationUser> userManager,
        IEcontService econtService, IOptions<EcontApiSettings> econtApiSettings,
        IEmailService emailService, IEmailUserProvider emailUserProvider,
        IAdminNotificationPreferencesService adminNotificationPreferencesService, IUrlProvider urlProvider,
        ILogger<OrderService> logger, IFastCacheService cacheService)
    {
        Repository = repository;
        UserManager = userManager;
        EcontService = econtService;
        EcontApiSettings = econtApiSettings.Value;
        OrderDataService = orderDataService;
        EmailService = emailService;
        EmailUserProvider = emailUserProvider;
        AdminNotificationPreferencesService = adminNotificationPreferencesService;
        UrlProvider = urlProvider;
        CacheService = cacheService;
        _logger = logger;
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

        OrderSummaryDto result = new()
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
        _logger.LogInformation("Successfully retrieved order summary for User {UserId}.", userId);

        return result;
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
            OrderSum = cartItems.Sum(ci => productPricesWithDiscounts[ci.ProductId] * ci.Quantity),
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
                   Currency = ci.Product.Currency
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

        foreach (CartItem item in cartItems)
        {
            item.Product.StockQuantity -= item.Quantity;
            if (item.Product.StockQuantity < MaxItemQuantityPerOrder)
            {
                await CacheService.RemoveAsync(ProductCardKey(item.ProductId));
                await CacheService.RemoveAsync(ProductDetailsKey(item.ProductId));
            }
        }

        Repository.DeleteRange(cartItems);
        await Repository.SaveChangesAsync();
        await NotifyAdminsForNewOrderAsync(order);

        _logger.LogInformation("Successfully created Order {OrderId} for User {UserId}.", order.Id, userId);
        return ServiceResult<Guid>.Ok(order.Id);
    }

    public async Task<ICollection<MyOrderDto>> GetUserOrders(string userId, int page)
    {
        MyOrderDto[] result = await OrderDataService.GetUserOrdersPagedReadonlyAsync(userId, page);

        _logger.LogInformation("Successfully retrieved orders for User {UserId}, Page {Page}. Count: {Count}", userId, page, result.Length);

        return result;
    }

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

        _logger.LogInformation("Successfully retrieved details for Order {OrderId}, User {UserId}.", orderId, userId);
        return ServiceResult<OrderDetailsDto>.Ok(model);
    }

    public async Task<bool> CanAccessSuccessPage(string userId, Guid orderId)
        => await Repository
            .WhereReadonly<Order>(o => o.Id == orderId && o.UserId == userId && o.Date > DateTime.UtcNow.AddSeconds(-SuccessPageMaxViewDelaySeconds))
            .AnyAsync();

    private async Task NotifyAdminsForNewOrderAsync(Order order)
    {
        try
        {
            ICollection<string> adminEmails = await AdminNotificationPreferencesService
                .GetAdminEmailsForNotificationAsync(AdminNotificationType.NewOrder);

            if (!adminEmails.Any())
                return;

            string orderDetailsUrl = UrlProvider.GetAdminOrderDetailsPageUrl(order.Id);
            string subject = $"Нова поръчка {order.Shipment.OrderNumber}";
            string body = $@"
                <!DOCTYPE html>
                <html lang=""bg"">
                <head>
                    <meta charset=""UTF-8"">
                </head>
                <body style=""margin:0;padding:0;background:#f0f0f0;font-family:'Segoe UI',Arial,sans-serif;color:#181717;"">
                    <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""padding:20px;"">
                        <tr>
                            <td align=""center"">
                                <table role=""presentation"" width=""100%"" style=""max-width:600px;background:#ffffff;border:1px solid #e8d7e8;border-radius:10px;overflow:hidden;"">
                                    <tr>
                                        <td style=""background:#3a053a;padding:16px;text-align:center;"">
                                            <h2 style=""margin:0;color:#fcfcfc;font-size:18px;"">Нова поръчка в системата</h2>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style=""padding:24px;"">
                                            <p style=""margin:0 0 8px 0;""><strong>Номер:</strong> {order.Shipment.OrderNumber}</p>
                                            <p style=""margin:0 0 8px 0;""><strong>Клиент:</strong> {order.Shipment.Face}</p>
                                            <p style=""margin:0 0 8px 0;""><strong>Имейл:</strong> {order.Shipment.Email}</p>
                                            <p style=""margin:0 0 16px 0;""><strong>Телефон:</strong> {order.Shipment.Phone}</p>
                                            <p style=""margin:0 0 18px 0;""><strong>Създадена на:</strong> {order.Date:dd.MM.yyyy HH:mm} UTC</p>
                                            <a href=""{orderDetailsUrl}"" style=""background:#3a053a;color:#fcfcfc;padding:11px 20px;border-radius:8px;text-decoration:none;font-weight:700;display:inline-block;"">
                                                Отвори поръчката в админ панела
                                            </a>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>";

            await EmailService.SendEmailsBulkAsync(
                EmailUserProvider.GetNotificationsUser(),
                adminEmails,
                subject,
                body,
                true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send admin new-order notifications for order {OrderId}.", order.Id);
        }
    }
}
