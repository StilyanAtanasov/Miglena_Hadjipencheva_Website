using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dto;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Data.Shared;
using MHAuthorWebsite.Web.ViewModels.Order;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static MHAuthorWebsite.GCommon.ApplicationRules.Application;

namespace MHAuthorWebsite.Core;

public class OrderService : IOrderService
{
    private readonly IApplicationRepository _repository;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IEcontService _econtService;

    public OrderService(IApplicationRepository repository, UserManager<IdentityUser> userManager, IEcontService econtService)
    {
        _repository = repository;
        _userManager = userManager;
        _econtService = econtService;
    }

    public async Task<OrderDetailsViewModel> GetOrderDetails(string userId)
    {
        IdentityUser user = (await _userManager.FindByIdAsync(userId))!;

        ICollection<SelectedProductViewModel> selectedProducts = await _repository
            .WhereReadonly<CartItem>(ci => ci.Cart.UserId == userId && ci.IsSelected && ci.Product.IsPublic && ci.Product.StockQuantity >= ci.Quantity)
            .Include(ci => ci.Cart)
            .Include(ci => ci.Product)
                .ThenInclude(p => p.Images)
            .Select(ci => new SelectedProductViewModel
            {
                ImageUrl = ci.Product.Images.FirstOrDefault()!.ThumbnailUrl!,
                Name = ci.Product.Name,
                TotalPrice = ci.Product.Price * ci.Quantity,
                Quantity = ci.Quantity
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
        CartItem[] cartItems = await _repository
            .Where<CartItem>(ci => ci.Cart.UserId == userId && ci.IsSelected && ci.Product.IsPublic && ci.Product.StockQuantity >= ci.Quantity)
            .Include(ci => ci.Cart)
            .Include(ci => ci.Product)
            .ToArrayAsync();

        EcontOrderDto order = new()
        {
            OrderNumber = Guid.NewGuid().ToString("N"),
            Status = "New",
            OrderTime = DateTime.UtcNow.ToString("o"),
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
                    TotalWeight = 0.5m * i.Quantity, // TODO USE REAL WEIGHT
                    HideCount = 0,
                    Url = i.Product.Id.ToString(),
                    Sku = i.Product.Id.ToString()
                }).ToArray()
        };

        ServiceResult sr = await _econtService.PlaceOrderAsync(order);

        foreach (CartItem item in cartItems) item.Product.StockQuantity -= item.Quantity;

        _repository.DeleteRange(cartItems);
        await _repository.SaveChangesAsync();

        return ServiceResult.Ok();
    }
}