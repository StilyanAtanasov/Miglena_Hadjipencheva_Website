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
using static MHAuthorWebsite.GCommon.ApplicationRules.Application;

namespace MHAuthorWebsite.Core.Admin;

public class AdminOrderService : OrderService, IAdminOrderService
{
    private readonly IAdminEcontService _adminEcontService;

    public AdminOrderService
        (IApplicationRepository repository,
        UserManager<IdentityUser> userManager,
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
        Order? order = await Repository
            .All<Order>()
            .Include(o => o.Shipment)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null) return ServiceResult.BadRequest();

        EcontOrderDto orderDto = new()
        {
            Id = order.Shipment.CourierShipmentId,
            OrderNumber = order.Shipment.OrderNumber,
        };

        ServiceResult<EcontShipmentStatusDto> sr = await _adminEcontService.CreateAWBAsync(orderDto);
        if (!sr.Success) return ServiceResult.Failure();

        order.Status = OrderStatus.Accepted;
        order.Shipment.ShipmentNumber = sr.Result!.ShipmentNumber;

        await Repository.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> RejectOrderAsync(Guid orderId)
    {
        Order? order = await Repository
            .All<Order>()
            .Include(o => o.Shipment)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null) return ServiceResult.BadRequest();

        EcontOrderDto orderDto = new()
        {
            Id = order.Shipment.CourierShipmentId,
            OrderNumber = order.Shipment.OrderNumber,
        };

        ServiceResult sr = await _adminEcontService.DeleteLabelAsync(orderDto);
        if (!sr.Success) return ServiceResult.Failure();

        // TODO finish this method

        return ServiceResult.Ok();
    }
}