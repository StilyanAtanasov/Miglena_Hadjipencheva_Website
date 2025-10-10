using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dto;
using MHAuthorWebsite.Data.Common.Extensions;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Data.Models.Enums;
using MHAuthorWebsite.Data.Shared;
using MHAuthorWebsite.Data.Shared.Filters;
using MHAuthorWebsite.Data.Shared.Filters.Criteria;
using MHAuthorWebsite.Web.ViewModels.Admin.Order;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static MHAuthorWebsite.GCommon.ApplicationRules.OrderSystemEventsMessages;

namespace MHAuthorWebsite.Core.Admin;

public class AdminOrderService : OrderService, IAdminOrderService
{
    private readonly IAdminEcontService _adminEcontService;

    public AdminOrderService
        (IApplicationRepository repository,
        UserManager<ApplicationUser> userManager,
        IEcontService econtService,
        IAdminEcontService adminEcontService)
        : base(repository, userManager, econtService)
        => _adminEcontService = adminEcontService;

    public async Task<ICollection<AllOrdersListItemViewModel>> GetAllOrders(AllOrdersFilterCriteria filter)
        => await Repository
            .AllReadonly(new AllOrdersFilter(filter))
            .Include(o => o.OrderedProducts)
            .Include(o => o.User)
            .Include(o => o.Shipment)
            .Select(o => new AllOrdersListItemViewModel
            {
                Id = o.Id,
                CustomerName = o.Shipment.Face, // TODO Check if user has their data deleted
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
            Status = order.Status.GetDisplayName(),
            Products = order.OrderedProducts
                 .Select(op => new AdminOrderProductDetailsViewModel
                 {
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