using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dtos.Cart;
using MHAuthorWebsite.Web.Utils.Attributes;
using MHAuthorWebsite.Web.Utils.Enums;
using MHAuthorWebsite.Web.ViewModels.Cart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace MHAuthorWebsite.Web.Controllers;

public class CartController : BaseController
{
    private readonly ICartService _cartService;
    public CartController(ICartService cartService) => _cartService = cartService;

    [SecurityHeaders(CspFeature.Notifications)]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!IsUserAuthenticated()) return Unauthorized();

        CartDto cartDto = await _cartService.GetCartReadonlyAsync(GetUserId()!);
        CartViewModel cart = new CartViewModel
        {
            Items = cartDto.Items
                .Select(i => new CartItemViewModel
                {
                    ItemId = i.ItemId,
                    ProductId = i.ProductId,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity,
                    MaxOrderQuantityForProduct = i.MaxOrderQuantityForProduct,
                    IsSelected = i.IsSelected,
                    Category = i.Category,
                    IsAvailable = i.IsAvailable,
                    IsDiscontinued = i.IsDiscontinued,
                    Name = i.Name,
                    ThumbnailAlt = i.ThumbnailAlt,
                    ThumbnailUrl = i.ThumbnailUrl,
                    UnitDiscountedPrice = i.UnitDiscountedPrice,
                }).ToList()
        };

        return View(cart);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Add([FromBody] AddCartItemViewModel model)
    {
        if (!IsUserAuthenticated()) return StatusCode(401);
        if (model.ProductId == Guid.Empty || model.Quantity <= 0)
            return BadRequest(new Dictionary<string, string> { ["error"] = "Невалидни данни за продукта." });

        ServiceResult result = await _cartService.AddItemToCartAsync(GetUserId()!, model.ProductId, model.Quantity);
        if (result.IsBadRequest) return BadRequest(result.Errors);
        if (!result.Success) return StatusCode(500);

        return StatusCode(200);
    }

    [HttpPost("Cart/Remove/{itemId}")]
    public async Task<IActionResult> Remove([FromRoute] Guid itemId)
    {
        if (!IsUserAuthenticated()) return Unauthorized();

        ServiceResult r = await _cartService.RemoveFromCartAsync(GetUserId()!, itemId);
        if (r.IsBadRequest) return BadRequest(r.Errors);

        return StatusCode(200);
    }

    [HttpPost("Cart/UpdateQuantity")]
    public async Task<IActionResult> UpdateQuantity([FromBody] UpdateItemQuantityViewModel model)
    {
        if (!IsUserAuthenticated()) return Unauthorized();

        ServiceResult<UpdatedItemQuantityDto> sr = await _cartService
            .UpdateItemQuantityAsync(GetUserId()!, model.ItemId, model.Quantity);
        if (sr.IsBadRequest) return BadRequest(sr.Errors);
        if (!sr.Success) return StatusCode(500);

        return Json(new
        {
            lineTotal = sr.Result!.LineTotal.ToString("F2"),
            cartTotal = sr.Result.Total.ToString("F2")
        });
    }

    [HttpPost("Cart/UpdateIsSelected")]
    public async Task<IActionResult> UpdateIsSelected([FromBody] UpdateItemIsSelectedViewModel model)
    {
        if (!IsUserAuthenticated()) return Unauthorized();

        ServiceResult sr = await _cartService
            .UpdateIsSelectedAsync(GetUserId()!, model.ItemId, model.IsSelected);
        if (sr.IsBadRequest) return BadRequest(sr.Errors);

        return StatusCode(200);
    }
}