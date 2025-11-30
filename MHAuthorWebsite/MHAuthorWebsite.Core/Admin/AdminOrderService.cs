using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dto;
using MHAuthorWebsite.Core.EmailConfiguration.Contracts;
using MHAuthorWebsite.Data.Common.Extensions;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Data.Models.Enums;
using MHAuthorWebsite.Data.Shared;
using MHAuthorWebsite.Data.Shared.Filters;
using MHAuthorWebsite.Data.Shared.Filters.Criteria;
using MHAuthorWebsite.Web.ViewModels.Admin.Order;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using static MHAuthorWebsite.GCommon.ApplicationRules.OrderSystemEventsMessages;

namespace MHAuthorWebsite.Core.Admin;

public class AdminOrderService : OrderService, IAdminOrderService
{
    private readonly IAdminEcontService _adminEcontService;
    private readonly IEmailService _emailService;
    private readonly IEmailUserProvider _emailUserProvider;
    private readonly LinkGenerator _linkGenerator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AdminOrderService
    (IApplicationRepository repository,
        UserManager<ApplicationUser> userManager,
        IEcontService econtService,
        IAdminEcontService adminEcontService,
        IConfiguration configuration,
        IEmailService emailService,
        IEmailUserProvider emailUserProvider,
        LinkGenerator linkGenerator,
        IHttpContextAccessor httpContextAccessor)
        : base(repository, userManager, econtService, configuration)
    {
        _adminEcontService = adminEcontService;
        _emailService = emailService;
        _emailUserProvider = emailUserProvider;
        _linkGenerator = linkGenerator;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ICollection<AllOrdersListItemViewModel>> GetAllOrders(AllOrdersFilterCriteria filter)
        => await Repository
            .AllReadonly(new AllOrdersFilter(filter))
            .Include(o => o.OrderedProducts)
            .Include(o => o.User)
            .Include(o => o.Shipment)
            .Select(o => new AllOrdersListItemViewModel
            {
                Id = o.Id,
                CustomerName = o.Shipment.Face,
                OrderDate = o.Date,
                TotalAmount = o.OrderedProducts.Sum(op => op.UnitPrice * op.Quantity),
                Currency = o.Shipment.Currency,
                Status = o.Status
            })
            .ToArrayAsync();

    public async Task<ServiceResult<AdminOrderDetailsViewModel>> GetOrderDetailsAsync(Guid orderId)
    {
        Order? order = await Repository
            .WhereReadonly<Order>(o => o.Id == orderId)
            .Include(o => o.OrderedProducts)
                .ThenInclude(op => op.Product)
                    .ThenInclude(p => p.Thumbnail)
                        .ThenInclude(t => t.Image)
            .Include(o => o.Shipment)
                .ThenInclude(s => s.Events)
            .Include(o => o.Shipment)
                .ThenInclude(s => s.Services)
            .FirstOrDefaultAsync();

        if (order == null) return ServiceResult<AdminOrderDetailsViewModel>.NotFound();

        AdminOrderDetailsViewModel model = new()
        {
            OrderId = order.Id,
            OrderDate = order.Date,
            Status = order.Status,
            Products = order.OrderedProducts
                 .Select(op => new AdminOrderProductDetailsViewModel
                 {
                     Id = op.ProductId,
                     ImageUrl = op.Product.Thumbnail.Image.ImageUrl,
                     ProductName = op.Product.Name,
                     UnitPrice = op.UnitPrice,
                     Quantity = op.Quantity,
                 })
                 .ToArray(),
            Shipment = new AdminOrderShipmentDetailsViewModel
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
                     .Select(e => new AdminOrderShipmentEventViewModel
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
                    .Select(s => new AdminOrderShipmentServiceViewModel
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

        return ServiceResult<AdminOrderDetailsViewModel>.Ok(model);
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

        string url = _linkGenerator.GetUriByAction(
            httpContext: _httpContextAccessor.HttpContext!,
            action: "OrderDetails",
            controller: "Order",
            values: new { orderId }
        )!;

        string contactsUrl = _linkGenerator.GetUriByAction(
            httpContext: _httpContextAccessor.HttpContext!,
            action: "Index",
            controller: "Contacts"
        )!;

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

        string contactsUrl = _linkGenerator.GetUriByAction(
            httpContext: _httpContextAccessor.HttpContext!,
            action: "Index",
            controller: "Contacts"
        )!;

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

        string contactsUrl = _linkGenerator.GetUriByAction(
            httpContext: _httpContextAccessor.HttpContext!,
            action: "Index",
            controller: "Contacts"
        )!;

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

        return ServiceResult.Ok();
    }

    private async Task RestoreProducts(IApplicationRepository repository, Guid orderId, bool saveChanges = true)
    {
        OrderProduct[] orderedProducts = await repository
            .Where<OrderProduct>(op => op.OrderId == orderId)
            .Include(op => op.Product)
            .ToArrayAsync();

        foreach (OrderProduct orderedProduct in orderedProducts)
            orderedProduct.Product.StockQuantity += orderedProduct.Quantity;

        if (saveChanges) await repository.SaveChangesAsync();
    }

    private async Task<(Order?, EcontOrderDto?)> PrepareOrderDto(Guid orderId)
    {
        Order? order = await Repository
            .All<Order>()
            .Include(o => o.Shipment)
            .Include(o => o.OrderedProducts)
                .ThenInclude(op => op.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null) return (null, null);

        EcontOrderDto orderDto = new()
        {
            Id = order.Shipment.CourierShipmentId,
            Status = order.Status.GetDisplayName(),
            OrderNumber = order.Shipment.OrderNumber,
            Items = order.OrderedProducts
                .Select(i => new OrderItemDto
                {
                    Count = i.Quantity,
                    Name = i.Product.Name,
                    TotalPrice = i.UnitPrice * i.Quantity,
                    TotalWeight = i.Product.Weight * i.Quantity
                }).ToArray()
            // NOTE: The API requires Items to update the order info.
            // TODO make the logic around this cleaner
        };

        return (order, orderDto);
    }
}