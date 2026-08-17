using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Admin.Contracts.DataServices;
using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Common.Extensions;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Configuration.EcontApi;
using MHAuthorWebsite.Core.Configuration.EmailConfiguration.Contracts;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Contracts.DataServices;
using MHAuthorWebsite.Core.Dtos.Admin.Order;
using MHAuthorWebsite.Core.Dtos.Order;
using MHAuthorWebsite.Core.Extensions;
using MHAuthorWebsite.Core.Filters;
using MHAuthorWebsite.Core.Filters.Criteria;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using MHAuthorWebsite.Core.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static MHAuthorWebsite.GCommon.ApplicationRules.OrderSystemEventsMessages;

namespace MHAuthorWebsite.Core.Admin;

public class AdminOrderService : OrderService, IAdminOrderService
{
    private readonly IAdminEcontService _adminEcontService;
    private readonly IEmailService _emailService;
    private readonly IEmailUserProvider _emailUserProvider;
    private readonly IUrlProvider _urlProvider;
    private readonly IAdminOrderDataService _adminOrderDataService;
    private readonly ILogger<AdminOrderService> _logger;

    public AdminOrderService
    (IApplicationRepository repository,
        IAdminOrderDataService adminOrderDataService,
        UserManager<ApplicationUser> userManager,
        IEcontService econtService,
        IOrderDataService orderDataService,
        IAdminEcontService adminEcontService,
        IOptions<EcontApiSettings> econtSettings,
        IEmailService emailService,
        IEmailUserProvider emailUserProvider,
        IAdminNotificationPreferencesService adminNotificationPreferencesService,
        IUrlProvider urlProvider,
        ILogger<AdminOrderService> logger,
        ILogger<OrderService> baseLogger,
        IFastCacheService cacheService)
        : base(repository, orderDataService, userManager, econtService, econtSettings, emailService, emailUserProvider, adminNotificationPreferencesService, urlProvider, baseLogger, cacheService)
    {
        _adminEcontService = adminEcontService;
        _emailService = emailService;
        _emailUserProvider = emailUserProvider;
        _urlProvider = urlProvider;
        _adminOrderDataService = adminOrderDataService;
        _logger = logger;
    }

    public async Task<ICollection<AllOrdersListItemDto>> GetAllOrders(AllOrdersFilterCriteria filter)
    {
        AllOrdersListItemDto[] result = await _adminOrderDataService.GetAllOrdersByFilterReadonlyAsync(filter);

        _logger.LogInformation("Successfully retrieved all orders from admin panel. Count: {Count}", result.Length);

        return result;
    }

    public async Task<ServiceResult<AdminOrderDetailsDto>> GetOrderDetailsAsync(Guid orderId)
    {
        Order? order = await _adminOrderDataService.GetOrderByIdForOrderDetailsReadonlyAsync(orderId);

        if (order == null) return ServiceResult<AdminOrderDetailsDto>.NotFound();

        AdminOrderDetailsDto model = new()
        {
            OrderId = order.Id,
            OrderDate = order.Date,
            Status = order.Status,
            Products = order.OrderedProducts
                 .Select(op => new AdminOrderProductDetailsDto
                 {
                     Id = op.ProductId,
                     ImageUrl = op.Product.Thumbnail.Image.ImageUrl,
                     ProductName = op.Product.Name,
                     UnitPrice = op.UnitPrice,
                     Quantity = op.Quantity,
                 })
                 .ToArray(),
            Shipment = new AdminOrderShipmentDetailsDto
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
                     .Select(e => new AdminOrderShipmentEventDto
                     {
                         CityName = e.CityName,
                         DestinationDetails = e.DestinationDetails!,
                         OfficeName = e.OfficeName,
                         Time = e.Time,
                     })
                     .ToArray(),
                ExpectedDeliveryDate = order.Shipment.ExpectedDeliveryDate,
                Currency = order.Shipment.Currency,
                AwbUrl = order.Shipment.AwbUrl,
                Services = order.Shipment.Services
                    .Select(s => new AdminOrderShipmentServiceDto
                    {
                        Count = s.Count,
                        Currency = s.Currency,
                        Price = s.Price,
                        Description = s.Description,
                        PaymentSide = s.PaymentSide,
                        Type = s.Type
                    })
                    .ToArray()
            }
        };

        _logger.LogInformation("Successfully retrieved admin details for order {OrderId}.", orderId);
        return ServiceResult<AdminOrderDetailsDto>.Ok(model);
    }

    public async Task<ServiceResult> AcceptOrderAsync(Guid orderId)
    {
        (Order? order, EcontOrderDto? orderDto) = await PrepareOrderDto(orderId);

        if (order == null || order.Status != OrderStatus.InReview) return ServiceResult.BadRequest();

        orderDto!.Status = OrderStatus.Accepted.GetDisplayName();

        ServiceResult<EcontOrderDto> orderInfoUpdateResult = await EcontService.UpdateOrderAsync(orderDto);
        if (!orderInfoUpdateResult.Success) return ServiceResult.Failure();

        ServiceResult<EcontShipmentStatusDto> awbCreationResult = await _adminEcontService.CreateAwbAsync(orderDto);
        if (!awbCreationResult.Success)
        {
            orderDto.Status = OrderStatus.InReview.GetDisplayName();
            await EcontService.UpdateOrderAsync(orderDto);
            return ServiceResult.Failure();
        }

        await Repository.AddAsync(new ShipmentEvent
        {
            Time = DateTime.UtcNow,
            Source = ShipmentEventSource.System,
            DestinationDetails = Accepted,
            ShipmentId = order.Shipment.Id
        });

        EcontShipmentStatusDto shipmentInfo = awbCreationResult.Result!;

        order.Status = OrderStatus.Accepted;
        order.Shipment.ShipmentNumber = shipmentInfo.ShipmentNumber;
        order.Shipment.AwbUrl = shipmentInfo.PdfUrl;

        order.Shipment.Services = shipmentInfo.Services
            .Select(s => new ShipmentService
            {
                Count = s.Count,
                Currency = s.Currency,
                Price = s.Price,
                Description = s.Description,
                PaymentSide = s.PaymentSide,
                Type = s.Type
            })
            .ToArray();

        if (shipmentInfo.TrackingEvents is not null)
        {
            ShipmentEvent[] events = shipmentInfo.TrackingEvents
            .Select(e => new ShipmentEvent
            {
                DestinationType = e.DestinationType,
                DestinationDetails = e.DestinationDetails,
                CityName = e.CityName,
                OfficeName = e.OfficeName,
                Time = DateTime.Parse(e.Time!),
                Source = ShipmentEventSource.Econt
            })
            .ToArray();

            await Repository.AddRangeAsync(events);
        }

        await Repository.SaveChangesAsync();

        string userEmail = order.Shipment.Email;

        string url = _urlProvider.GetOrderDetailsPageUrl(orderId);
        string contactsUrl = _urlProvider.GetContactsPageUrl();

        await _emailService.SendEmailAsync(
             _emailUserProvider.GetNotificationsUser(),
             userEmail,
             "Поръчката Ви е приета!",
             $@"<!DOCTYPE html>
            <html lang=""bg"">
            <head>
                <meta charset=""UTF-8"">
                <title>Поръчката Ви е приета</title>
            </head>
            <body style=""margin:0;padding:0;background:rgba(240,240,240,0.721);font-family:'Sofia Sans Condensed',Arial,sans-serif;color:rgba(24,23,23,0.879);"">
                <div style=""max-width:600px;margin:30px auto;background:#fcfcfc;padding:30px;border-radius:12px;box-shadow:0 4px 14px rgba(0,0,0,0.08);"">

                    <h2 style=""color:rgb(34,201,34);margin-top:0;"">Вашата поръчка е приета!</h2>

                    <p>Уважаеми/Уважаема {order.Shipment.Face},</p>

                    <p>Вашата поръчка беше успешно приета и вече се подготвя да бъде изпратена!</p>

                    <p>
                        Можете да следите пратката си като кликнете 
                        <a href=""{url}"" style=""color:rgb(39,103,231);font-weight:bold;"">тук</a>.
                    </p>

                    <p style=""margin:20px 0;font-size:15px;color:rgba(24,23,23,0.879);"">
                        Ако имате въпроси или се нуждаете от съдействие, не се колебайте да се свържете с нас.
                    </p>
                    <div style=""margin:30px 0;"">
                        <a href=""{contactsUrl}"" 
                           style=""background:rgb(39,103,231);color:white;padding:12px 22px;border-radius:8px;text-decoration:none;font-weight:bold;display:inline-block;"">
                           Свържете се с нас
                        </a>
                    </div>

                    <hr style=""border:0;border-top:1px solid #ccc;margin:30px 0;"">

                    <p style=""font-size:12px;color:rgba(97,97,97,0.923);"">
                        Това е автоматично съобщение. Моля, не отговаряйте на него.
                    </p>

                </div>
            </body>
            </html>",
             true);

        _logger.LogInformation("Admin accepted order {OrderId}.", orderId);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> RejectOrderAsync(Guid orderId)
    {
        (Order? order, EcontOrderDto? orderDto) = await PrepareOrderDto(orderId);

        if (order == null || order.Status != OrderStatus.InReview) return ServiceResult.BadRequest();

        orderDto!.Status = OrderStatus.Rejected.GetDisplayName();

        ServiceResult<EcontOrderDto> orderInfoUpdateResult = await EcontService.UpdateOrderAsync(orderDto);
        if (!orderInfoUpdateResult.Success) return ServiceResult.Failure();

        await Repository.AddAsync(new ShipmentEvent()
        {
            Time = DateTime.UtcNow,
            Source = ShipmentEventSource.System,
            DestinationDetails = Rejected,
            ShipmentId = order.Shipment.Id
        });

        order.Status = OrderStatus.Rejected;
        await RestoreProducts(Repository, order.Id, false);

        await Repository.SaveChangesAsync();

        string userEmail = order.Shipment.Email;
        string contactsUrl = _urlProvider.GetContactsPageUrl();

        await _emailService.SendEmailAsync(
            _emailUserProvider.GetNotificationsUser(),
            userEmail,
            "Поръчката Ви беше отхвърлена",
            $@"<!DOCTYPE html>
            <html lang=""bg"">
            <head>
                <meta charset=""UTF-8"">
                <title>Поръчката Ви беше отказана</title>
            </head>
            <body style=""margin:0;padding:0;background:rgba(240,240,240,0.721);font-family:'Sofia Sans Condensed',Arial,sans-serif;color:rgba(24,23,23,0.879);"">
                <div style=""max-width:600px;margin:30px auto;background:#fcfcfc;padding:30px;border-radius:12px;box-shadow:0 4px 14px rgba(0,0,0,0.08);"">

                    <h2 style=""color:rgb(255,73,73);margin-top:0;"">Вашата поръчка е отказана</h2>

                    <p>Уважаеми/Уважаема {order.Shipment.Face},</p>

                    <p>
                        За съжаление поръчката Ви не може да бъде изпълнена.  
                        Ако имате въпроси или желаете допълнителна информация, можете да се свържете с нас.
                    </p>

                    <p style=""margin:20px 0;font-size:15px;color:rgba(24,23,23,0.879);"">
                        Ако имате въпроси или се нуждаете от съдействие, не се колебайте да се свържете с нас.
                    </p>
                    <div style=""margin:30px 0;"">
                        <a href=""{contactsUrl}"" 
                           style=""background:rgb(58,5,58);color:white;padding:12px 22px;border-radius:8px;text-decoration:none;font-weight:bold;display:inline-block;"">
                           Свържете се с нас
                        </a>
                    </div>

                    <hr style=""border:0;border-top:1px solid #ccc;margin:30px 0;"">

                    <p style=""font-size:12px;color:rgba(97,97,97,0.923);"">
                        Това е автоматично съобщение. Моля, не отговаряйте на него.
                    </p>

                </div>
            </body>
            </html>",
            true);

        _logger.LogInformation("Admin rejected order {OrderId}.", orderId);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> TerminateOrderAsync(Guid orderId)
    {
        (Order? order, EcontOrderDto? orderDto) = await PrepareOrderDto(orderId);

        if (order == null || order.Status != OrderStatus.Accepted) return ServiceResult.BadRequest();

        orderDto!.Status = OrderStatus.Terminated.GetDisplayName();

        ServiceResult sr = await _adminEcontService.DeleteLabelAsync(orderDto);
        if (!sr.Success) return ServiceResult.Failure();

        ServiceResult<EcontOrderDto> orderInfoUpdateResult = await EcontService.UpdateOrderAsync(orderDto);
        if (!orderInfoUpdateResult.Success) return ServiceResult.Failure();

        await Repository.AddAsync(new ShipmentEvent
        {
            Time = DateTime.UtcNow,
            Source = ShipmentEventSource.System,
            DestinationDetails = Terminated,
            ShipmentId = order.Shipment.Id
        });

        order.Status = OrderStatus.Terminated;
        await RestoreProducts(Repository, order.Id, false);

        await Repository.SaveChangesAsync();

        string userEmail = order.Shipment.Email;
        string contactsUrl = _urlProvider.GetContactsPageUrl();

        await _emailService.SendEmailAsync(
            _emailUserProvider.GetNotificationsUser(),
            userEmail,
            "Поръчката Ви беше прекратена",
            $@"<!DOCTYPE html>
            <html lang=""bg"">
            <head>
                <meta charset=""UTF-8"">
                <title>Поръчката Ви беше прекратена</title>
            </head>
            <body style=""margin:0;padding:0;background:rgba(240,240,240,0.721);font-family:'Sofia Sans Condensed',Arial,sans-serif;color:rgba(24,23,23,0.879);"">
                <div style=""max-width:600px;margin:30px auto;background:#fcfcfc;padding:30px;border-radius:12px;box-shadow:0 4px 14px rgba(0,0,0,0.08);"">

                    <h2 style=""color:rgb(255,191,0);margin-top:0;"">Вашата поръчка беше прекратена</h2>

                    <p>Уважаеми/Уважаема {order.Shipment.Face},</p>

                    <p>
                        Поръчката Ви беше маркирана като приета, но впоследствие прекратена преди изпращане.  
                        Ако желаете да научите причината или да направите нова поръчка, екипът ни е на разположение.
                    </p>

                    <p style=""margin:20px 0;font-size:15px;color:rgba(24,23,23,0.879);"">
                        Ако имате въпроси или се нуждаете от съдействие, не се колебайте да се свържете с нас.
                    </p>
                    <div style=""margin:30px 0;"">
                        <a href=""{contactsUrl}"" 
                           style=""background:rgb(58,5,58);color:white;padding:12px 22px;border-radius:8px;text-decoration:none;font-weight:bold;display:inline-block;"">
                           Свържете се с нас
                        </a>
                    </div>

                    <hr style=""border:0;border-top:1px solid #ccc;margin:30px 0;"">

                    <p style=""font-size:12px;color:rgba(97,97,97,0.923);"">
                        Това е автоматично съобщение. Моля, не отговаряйте на него.
                    </p>

                </div>
            </body>
            </html>",
            true);

        _logger.LogInformation("Admin terminated order {OrderId}.", orderId);
        return ServiceResult.Ok();
    }

    private async Task RestoreProducts(IApplicationRepository repository, Guid orderId, bool saveChanges = true)
    {
        OrderProduct[] orderedProducts =
            await _adminOrderDataService.GetOrderProductsByOrderByIdForRestoringProductAsync(orderId);

        foreach (OrderProduct orderedProduct in orderedProducts)
            orderedProduct.Product.StockQuantity += orderedProduct.Quantity;

        if (saveChanges) await repository.SaveChangesAsync();
    }

    private async Task<(Order?, EcontOrderDto?)> PrepareOrderDto(Guid orderId)
    {
        Order? order = await _adminOrderDataService.GetOrderByIdForEditAndOrderDtoAsync(orderId);

        if (order == null) return (null, null);

        EcontOrderDto orderDto = new()
        {
            Id = order.Shipment.CourierShipmentId,
            Status = order.Status.GetDisplayName(),
            OrderNumber = order.Shipment.OrderNumber,
            Currency = order.Shipment.Currency,
            Items = order.OrderedProducts
                .Select(i => new OrderItemDto
                {
                    Count = i.Quantity,
                    Name = i.Product.Name,
                    TotalPrice = i.UnitPrice * i.Quantity,
                    TotalWeight = i.Product.Weight * i.Quantity
                }).ToArray()
        };

        return (order, orderDto);
    }
}
