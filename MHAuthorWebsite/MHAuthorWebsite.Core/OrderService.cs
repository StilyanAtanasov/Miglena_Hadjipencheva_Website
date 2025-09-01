using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Data.Shared;
using MHAuthorWebsite.Web.ViewModels.Order;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MHAuthorWebsite.Core;

public class OrderService : IOrderService
{
    private readonly IApplicationRepository _repository;
    private readonly UserManager<IdentityUser> _userManager;

    public OrderService(IApplicationRepository repository, UserManager<IdentityUser> userManager)
    {
        _repository = repository;
        _userManager = userManager;
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
}