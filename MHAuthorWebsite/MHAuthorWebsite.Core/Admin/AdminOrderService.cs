using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dto;
using MHAuthorWebsite.Data.Common.Extensions;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Data.Models.Enums;
using MHAuthorWebsite.Data.Shared;
using MHAuthorWebsite.Web.ViewModels.Admin.Order;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

    public async Task<ICollection<AllOrdersListItemViewModel>> GetAllOrders()
        => await Repository
            .AllReadonly<Order>()
            .Include(o => o.OrderedProducts)
            .Include(o => o.User)
            .Include(o => o.Shipment)
            .Select(o => new AllOrdersListItemViewModel
            {
                Id = o.Id,
                OrderNumber = o.Shipment.OrderNumber,
                CustomerName = o.User.UserName!, // TODO Check if user has their data deleted
                OrderDate = o.Date,
                TotalAmount = o.OrderedProducts.Sum(op => op.UnitPrice * op.Quantity),
                Status = o.Status.GetDisplayName()
            })
            .ToArrayAsync();

    public async Task<ServiceResult> AcceptOrderAsync(Guid orderId)
    {
        (Order? order, EcontOrderDto? orderDto) = await PrepareOrderDto(orderId);

        if (order == null || order.Status != OrderStatus.InReview) return ServiceResult.BadRequest();

        orderDto!.Status = OrderStatus.Accepted.GetDisplayName();

        ServiceResult<EcontOrderDto> orderInfoUpdateResult = await EcontService.UpdateOrderAsync(orderDto);
        if (!orderInfoUpdateResult.Success) return ServiceResult.Failure();

        ServiceResult<EcontShipmentStatusDto> awbCreationResult = await _adminEcontService.CreateAWBAsync(orderDto);
        if (!awbCreationResult.Success)
        {
            orderDto.Status = OrderStatus.InReview.GetDisplayName();
            await EcontService.UpdateOrderAsync(orderDto);
            return ServiceResult.Failure();
        }

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

        order.Shipment.Events = shipmentInfo.TrackingEvents
            .Select(e => new ShipmentEvent
            {
                DestinationType = e.DestinationType,
                DestinationDetails = e.DestinationDetails,
                CityName = e.CityName,
                OfficeName = e.OfficeName,
                Time = DateTime.Parse(e.Time!),
            })
            .ToArray();

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