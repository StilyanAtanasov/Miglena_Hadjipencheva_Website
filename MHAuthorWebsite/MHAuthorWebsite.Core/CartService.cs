using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Data.Shared;
using MHAuthorWebsite.Web.ViewModels.Cart;
using Microsoft.EntityFrameworkCore;
using static MHAuthorWebsite.GCommon.ApplicationRules.CacheDefaultDurations;
using static MHAuthorWebsite.GCommon.ApplicationRules.CacheKeys;
using static MHAuthorWebsite.GCommon.EntityConstraints.CartItem;

namespace MHAuthorWebsite.Core;

public class CartService : ICartService
{
    private readonly IFastCacheService _cache;
    private readonly IApplicationRepository _repository;
    private readonly IGlobalCacheKeysManagementService _globalCacheKeysManagementService;

    public CartService(IFastCacheService cacheService, IApplicationRepository repository, IGlobalCacheKeysManagementService globalCacheKeysManagementService)
    {
        _cache = cacheService;
        _repository = repository;
        _globalCacheKeysManagementService = globalCacheKeysManagementService;
    }

    public async Task<ServiceResult> AddItemToCartAsync(string userId, Guid productId, int quantity)
    {
        try
        {
            Product? product = await _repository.FindByExpressionAsync<Product>(p => p.Id == productId);
            if (product == null) return ServiceResult.BadRequest(new() { ["product"] = "Продуктът не съществува!" });

            if (product.StockQuantity < quantity)
                return ServiceResult.BadRequest(new()
                {
                    ["quantity"] =
                    $"Недостатъчно количество на продукта! Максимална поръчка от {product.StockQuantity} продукта!"
                    + (product.StockQuantity > 1 ? "a" : "") + "."
                });

            Cart? cart = await _repository.FindByExpressionAsync<Cart>(c => c.UserId == userId);
            if (cart == null)
            {
                cart = new Cart { UserId = userId, };
                await _repository.AddAsync(cart);
                await _repository.SaveChangesAsync();
            }

            CartItem? existingCartItem = await _repository.FindByExpressionAsync<CartItem>(ci => ci.CartId == cart.Id && ci.ProductId == productId);
            if (existingCartItem is not null)
            {
                existingCartItem.Quantity += quantity; // TODO Add validation for maximum quantity
                existingCartItem.Price = product.Price;

                _repository.Update(existingCartItem);

                await _repository.SaveChangesAsync();
                await InvalidateCacheAsync(userId);

                return ServiceResult.Ok();
            }

            await _repository.AddAsync<CartItem>(new()
            {
                ProductId = productId,
                Quantity = quantity,
                CartId = cart.Id,
                Price = product.Price,
                IsSelected = IsSelectedDefaultValue
            });

            await _repository.SaveChangesAsync();

            await InvalidateCacheAsync(userId);

            return ServiceResult.Ok();
        }
        catch (Exception)
        {
            return ServiceResult.Failure();
        }
    }

    public async Task<CartViewModel> GetCartReadonlyAsync(string userId)
    {
        Guid stateId = await _globalCacheKeysManagementService.DiscountsGlobalStateId();
        CartViewModel? cachedCart = await _cache.GetAsync<CartViewModel>(CartKey(userId));
        if (cachedCart is not null && cachedCart.DiscountStateId == stateId) return cachedCart;

        Cart? cart = await _repository
            .AllReadonly<Cart>()
            .Where(c => c.UserId == userId)
            .IgnoreQueryFilters()
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                    .ThenInclude(p => p.ProductType)
            .Include(c => c.CartItems)
                 .ThenInclude(ci => ci.Product)
                    .ThenInclude(p => p.Thumbnail)
                        .ThenInclude(t => t.Image)
            .Include(c => c.CartItems)
                 .ThenInclude(ci => ci.Product)
                    .ThenInclude(p => p.Discounts)
            .FirstOrDefaultAsync();
        if (cart is null) return new CartViewModel();

        ICollection<CartItemViewModel> cartItems =
            cart.CartItems
            .Select(ci => new CartItemViewModel
            {
                ItemId = ci.Id,
                ProductId = ci.ProductId,
                IsSelected = ci.IsSelected,
                Name = ci.Product.Name,
                Category = ci.Product.ProductType.Name,
                Quantity = ci.Quantity,
                UnitPrice = ci.Price,
                UnitDiscountedPrice = ci.Product.Discounts
                    .FirstOrDefault(d => d.StartDate <= DateTime.Now && d.EndDate >= DateTime.Now)?.NewPrice,
                IsDiscontinued = ci.Product.IsDeleted || !ci.Product.IsPublic,
                IsAvailable = ci.Product is { StockQuantity: > 0, IsDeleted: false, IsPublic: true },
                ThumbnailUrl = ci.Product.Thumbnail.Image.ImageUrl,
                ThumbnailAlt = ci.Product.Thumbnail.Image.AltText,
            })
            .ToArray();

        CartViewModel cartViewModel = new CartViewModel
        {
            DiscountStateId = stateId,
            Items = cartItems
        };

        DateTime now = DateTime.Now;
        DateTime? earliestExpiration = cart.CartItems
            .SelectMany(ci => ci.Product.Discounts)
            .Where(d => d.EndDate > now)
            .OrderBy(d => d.EndDate)
            .Select(d => d.EndDate)
            .FirstOrDefault();

        TimeSpan cacheExpiration = earliestExpiration.HasValue && earliestExpiration.Value - now < TimeSpan.FromDays(CartTtlDays)
            ? earliestExpiration.Value - now
            : TimeSpan.FromDays(CartTtlDays);

        _cache.SetFireAndForget(CartKey(userId), cartViewModel, cacheExpiration);

        return cartViewModel;
    }

    public async Task<ServiceResult> RemoveFromCartAsync(string userId, Guid itemId)
    {
        Cart? cart = await _repository.FindByExpressionAsync<Cart>(c => c.UserId == userId, true, c => c.CartItems);

        if (cart == null) return ServiceResult.BadRequest(new() { ["cart"] = "User has nothing in cart!" });

        CartItem? cartItem = _repository
            .All<CartItem>()
            .IgnoreQueryFilters()
            .FirstOrDefault(c => c.CartId == cart.Id && c.Id == itemId);

        if (cartItem == null) return ServiceResult.BadRequest(new() { ["product"] = "User does not have the specified product in their cart!" });

        _repository.Delete(cartItem);
        if (cart.CartItems.Count == 1) _repository.Delete(cart);

        await _repository.SaveChangesAsync();

        await InvalidateCacheAsync(userId);

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult<UpdatedItemQuantityViewModel>> UpdateItemQuantityAsync(string userId, Guid itemId, int quantity)
    {
        Cart? cart = await _repository
            .All<Cart>()
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                    .ThenInclude(p => p.Discounts)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null) return ServiceResult<UpdatedItemQuantityViewModel>
            .BadRequest(new() { ["cart"] = "Потребителят няма нищо в количката!" });

        CartItem? cartItem = cart.CartItems
            .FirstOrDefault(c => c.CartId == cart.Id && c.Id == itemId);

        if (cartItem == null) return ServiceResult<UpdatedItemQuantityViewModel>
            .BadRequest(new() { ["product"] = "Продуктът не е намерен в количката!" });

        cartItem.Quantity = quantity;
        await _repository.SaveChangesAsync();

        await InvalidateCacheAsync(userId);

        return ServiceResult<UpdatedItemQuantityViewModel>.Ok(new()
        {
            LineTotal = cartItem.Quantity * (cartItem.Product.Discounts.FirstOrDefault(d => d.StartDate <= DateTime.UtcNow && d.EndDate >= DateTime.UtcNow)?.NewPrice ?? cartItem.Price),
            Total = cart.CartItems
                .Where(ci => ci.Product.StockQuantity > 0 && ci.IsSelected)
                .Sum(ci => ci.Quantity * (ci.Product.Discounts.FirstOrDefault(d => d.StartDate <= DateTime.UtcNow && d.EndDate >= DateTime.UtcNow)?.NewPrice ?? ci.Price))

            // Total price of all items in stock in the cart
            // (the non-public and deleted ones are excluded by default using a query filter)
        });
    }

    public async Task<ServiceResult> UpdateIsSelectedAsync(string userId, Guid itemId, bool isSelected)
    {
        Cart? cart = await _repository
            .All<Cart>()
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null) return ServiceResult
            .BadRequest(new() { ["cart"] = "Потребителят няма нищо в количката!" });

        CartItem? cartItem = cart.CartItems
            .FirstOrDefault(c => c.CartId == cart.Id && c.Id == itemId);

        if (cartItem == null) return ServiceResult
            .BadRequest(new() { ["product"] = "Продуктът не е намерен в количката!" });

        cartItem.IsSelected = isSelected;
        await _repository.SaveChangesAsync();

        await InvalidateCacheAsync(userId);

        return ServiceResult.Ok();
    }

    private async Task InvalidateCacheAsync(string userId)
        => await _cache.RemoveAsync(CartKey(userId));
}