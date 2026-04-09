using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Contracts.DataServices;
using MHAuthorWebsite.Core.Dtos.Cart;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using Microsoft.Extensions.Logging;
using static MHAuthorWebsite.GCommon.ApplicationRules.CacheDefaultDurations;
using static MHAuthorWebsite.GCommon.ApplicationRules.CacheKeys;
using static MHAuthorWebsite.GCommon.ApplicationRules.Order;
using static MHAuthorWebsite.GCommon.EntityConstraints.CartItem;

namespace MHAuthorWebsite.Core;

public class CartService : ICartService
{
    private readonly IFastCacheService _cache;
    private readonly IApplicationRepository _repository;
    private readonly ICartDataService _cartDataService;
    private readonly IGlobalCacheKeysManagementService _globalCacheKeysManagementService;
    private readonly ILogger<CartService> _logger;

    public CartService(ICartDataService cartDataService, IFastCacheService cacheService, IApplicationRepository repository, IGlobalCacheKeysManagementService globalCacheKeysManagementService, ILogger<CartService> logger)
    {
        _cache = cacheService;
        _repository = repository;
        _cartDataService = cartDataService;
        _globalCacheKeysManagementService = globalCacheKeysManagementService;
        _logger = logger;
    }

    public async Task<ServiceResult> AddItemToCartAsync(string userId, Guid productId, int quantity)
    {
        try
        {
            Product? product = await _repository.FindByExpressionAsync<Product>(p => p.Id == productId);
            if (product == null) return ServiceResult.BadRequest(new() { ["product"] = "Продуктът не съществува!" });

            if (quantity > MaxItemQuantityPerOrder)
                return ServiceResult.BadRequest(new()
                {
                    ["quantity"] = $"Максимална поръчка от {MaxItemQuantityPerOrder} продукта!"
                });

            if (product.StockQuantity < quantity)
                return ServiceResult.BadRequest(new()
                {
                    ["quantity"] =
                    $"Недостатъчно количество на продукта! Максимална поръчка от {product.StockQuantity} продукт"
                    + (product.StockQuantity > 1 ? "a" : "") + "!"
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
                if (existingCartItem.Quantity + quantity > MaxItemQuantityPerOrder)
                    return ServiceResult.BadRequest(new()
                    {
                        ["quantity"] = $"Надвишавате максималния лимит от {MaxItemQuantityPerOrder} продукта в количката Ви!"
                    });

                existingCartItem.Quantity += quantity;
                existingCartItem.Price = product.Price;
                existingCartItem.Currency = product.Currency;

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
                Currency = product.Currency,
                IsSelected = IsSelectedDefaultValue
            });

            await _repository.SaveChangesAsync();

            await InvalidateCacheAsync(userId);

            await InvalidateCacheAsync(userId);

            _logger.LogInformation("User {UserId} added {Quantity} items of Product {ProductId} to cart.", userId, quantity, productId);

            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding item to cart for User {UserId}, Product {ProductId}", userId, productId);
            return ServiceResult.Failure();
        }
    }

    public async Task<CartDto> GetCartReadonlyAsync(string userId)
    {
        Guid stateId = await _globalCacheKeysManagementService.DiscountsGlobalStateId();
        CartDto? cachedCart = await _cache.GetAsync<CartDto>(CartKey(userId));
        if (cachedCart is not null && cachedCart.DiscountStateId == stateId) return cachedCart;

        Cart? cart = await _cartDataService.GetCartByUserIdReadonlyAsync(userId);
        if (cart is null) return new CartDto();

        ICollection<CartItemDto> cartItems =
            cart.CartItems
            .Select(ci => new CartItemDto
            {
                ItemId = ci.Id,
                ProductId = ci.ProductId,
                IsSelected = ci.IsSelected,
                Name = ci.Product.Name,
                Category = ci.Product.ProductType.Name,
                Quantity = ci.Quantity,
                MaxOrderQuantityForProduct = Math.Min(MaxItemQuantityPerOrder, ci.Product.StockQuantity),
                UnitPrice = ci.Price,
                UnitDiscountedPrice = ci.Product.Discounts
                    .FirstOrDefault(d => d.StartDate <= DateTime.UtcNow && d.EndDate >= DateTime.UtcNow)?.NewPrice,
                IsDiscontinued = ci.Product.IsDeleted || !ci.Product.IsPublic,
                IsAvailable = ci.Product is { StockQuantity: > 0, IsDeleted: false, IsPublic: true },
                ThumbnailUrl = ci.Product.Thumbnail.Image.ImageUrl,
                ThumbnailAlt = ci.Product.Thumbnail.Image.AltText,
            })
            .ToArray();

        CartDto cartDto = new CartDto
        {
            DiscountStateId = stateId,
            Items = cartItems
        };

        DateTime now = DateTime.UtcNow;
        DateTime? earliestExpiration = cart.CartItems
            .SelectMany(ci => ci.Product.Discounts)
            .Where(d => d.EndDate > now)
            .OrderBy(d => d.EndDate)
            .Select(d => d.EndDate)
            .FirstOrDefault();

        TimeSpan cacheExpiration = earliestExpiration.HasValue && earliestExpiration.Value - now < TimeSpan.FromDays(CartTtlDays)
            ? earliestExpiration.Value - now
            : TimeSpan.FromDays(CartTtlDays);

        _cache.SetFireAndForget(CartKey(userId), cartDto, cacheExpiration);

        _cache.SetFireAndForget(CartKey(userId), cartDto, cacheExpiration);
        _logger.LogInformation("Successfully retrieved cart for User {UserId}. Items count: {Count}", userId, cartDto.Items.Count);
        return cartDto;
    }

    public async Task<ServiceResult> RemoveFromCartAsync(string userId, Guid itemId)
    {
        Cart? cart = await _repository.FindByExpressionAsync<Cart>(c => c.UserId == userId, true, c => c.CartItems);

        if (cart == null) return ServiceResult.BadRequest(new() { ["cart"] = "User has nothing in cart!" });

        CartItem? cartItem = cart.CartItems
            .FirstOrDefault(c => c.Id == itemId);

        if (cartItem == null) return ServiceResult.BadRequest(new() { ["product"] = "User does not have the specified product in their cart!" });

        _repository.Delete(cartItem);
        if (cart.CartItems.Count == 1) _repository.Delete(cart);

        await _repository.SaveChangesAsync();

        await InvalidateCacheAsync(userId);

        await InvalidateCacheAsync(userId);

        _logger.LogInformation("User {UserId} removed Item {ItemId} from cart.", userId, itemId);

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult<UpdatedItemQuantityDto>> UpdateItemQuantityAsync(string userId, Guid itemId, int quantity)
    {
        Cart? cart = await _cartDataService.GetCartForItemQuantityUpdateAsync(userId);

        if (cart == null) return ServiceResult<UpdatedItemQuantityDto>
            .BadRequest(new() { ["cart"] = "Потребителят няма нищо в количката!" });

        CartItem? cartItem = cart.CartItems
            .FirstOrDefault(c => c.CartId == cart.Id && c.Id == itemId);

        if (cartItem == null) return ServiceResult<UpdatedItemQuantityDto>
            .BadRequest(new() { ["product"] = "Продуктът не е намерен в количката!" });

        int maxOrderQuantityForProduct = Math.Min(MaxItemQuantityPerOrder, cartItem.Product.StockQuantity);
        if (quantity > maxOrderQuantityForProduct) return ServiceResult<UpdatedItemQuantityDto>
            .BadRequest(new()
            {
                ["quantity"] = $"Можете да поръчате максимум {maxOrderQuantityForProduct} броя от този продукт!"
            });

        cartItem.Quantity = quantity;
        await _repository.SaveChangesAsync();

        await InvalidateCacheAsync(userId);

        await InvalidateCacheAsync(userId);

        _logger.LogInformation("User {UserId} updated quantity for Item {ItemId} to {Quantity}. Total updated.", userId, itemId, quantity);

        return ServiceResult<UpdatedItemQuantityDto>.Ok(new()
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
        Cart? cart = await _cartDataService.GetCartForSelectionUpdateAsync(userId);

        if (cart == null) return ServiceResult
            .BadRequest(new() { ["cart"] = "Потребителят няма нищо в количката!" });

        CartItem? cartItem = cart.CartItems
            .FirstOrDefault(c => c.CartId == cart.Id && c.Id == itemId);

        if (cartItem == null) return ServiceResult
            .BadRequest(new() { ["product"] = "Продуктът не е намерен в количката!" });

        cartItem.IsSelected = isSelected;
        await _repository.SaveChangesAsync();

        await InvalidateCacheAsync(userId);

        await InvalidateCacheAsync(userId);

        _logger.LogInformation("User {UserId} updated selection for Item {ItemId} to {IsSelected}.", userId, itemId, isSelected);

        return ServiceResult.Ok();
    }

    private async Task InvalidateCacheAsync(string userId)
        => await _cache.RemoveAsync(CartKey(userId));
}