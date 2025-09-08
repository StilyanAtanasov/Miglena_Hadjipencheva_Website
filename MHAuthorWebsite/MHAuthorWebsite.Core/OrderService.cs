using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dto;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Data.Models.Enums;
using MHAuthorWebsite.Data.Shared;
using MHAuthorWebsite.Web.ViewModels.Order;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static MHAuthorWebsite.GCommon.ApplicationRules.Application;

namespace MHAuthorWebsite.Core;

public class OrderService : IOrderService
{
    protected readonly IApplicationRepository Repository;
    protected readonly UserManager<IdentityUser> UserManager;
    protected readonly IEcontService EcontService;

    public OrderService(IApplicationRepository repository, UserManager<IdentityUser> userManager, IEcontService econtService)
    {
        Repository = repository;
        UserManager = userManager;
        EcontService = econtService;
    }

    public async Task<OrderDetailsViewModel> GetOrderDetails(string userId)
    {
        IdentityUser user = (await UserManager.FindByIdAsync(userId))!;

        ICollection<SelectedProductViewModel> selectedProducts = await Repository
            .WhereReadonly<CartItem>(ci => ci.Cart.UserId == userId && ci.IsSelected && ci.Product.IsPublic && ci.Product.StockQuantity >= ci.Quantity)
            .Include(ci => ci.Cart)
            .Include(ci => ci.Product)
                .ThenInclude(p => p.Images)
            .Select(ci => new SelectedProductViewModel
            {
                ImageUrl = ci.Product.Images.FirstOrDefault()!.ThumbnailUrl!,
                Name = ci.Product.Name,
                TotalPrice = ci.Product.Price * ci.Quantity,
                Quantity = ci.Quantity,
                TotalWeight = ci.Product.Weight * ci.Quantity
            })
            .ToListAsync();

        return new OrderDetailsViewModel
        {
            UserData = new()
            {
                Email = user.Email!,
                Name = user.UserName!, // TODO Require real name
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
            Status = "New",
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

        ServiceResult<EcontOrderDto> sr = await EcontService.PlaceOrderAsync(orderDto);
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
                Currency = Currency
            },
        };

        await Repository.AddAsync(order);

        foreach (CartItem item in cartItems) item.Product.StockQuantity -= item.Quantity;

        Repository.DeleteRange(cartItems);
        await Repository.SaveChangesAsync();

        return ServiceResult.Ok();
    }
}