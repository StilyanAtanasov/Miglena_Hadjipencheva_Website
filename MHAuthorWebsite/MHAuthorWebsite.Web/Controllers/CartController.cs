using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Web.ViewModels.Cart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MHAuthorWebsite.Web.Controllers;

public class CartController : BaseController
{
    private readonly ICartService _cartService;
    public CartController(ICartService cartService) => _cartService = cartService;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!IsUserAuthenticated()) return Unauthorized();

        CartViewModel cart = await _cartService.GetCartReadonlyAsync(GetUserId()!);
        return View(cart);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Add([FromBody] AddCartItemViewModel model)
    {
        if (!IsUserAuthenticated()) return StatusCode(401);
        if (model.ProductId == Guid.Empty || model.Quantity <= 0) return BadRequest("Invalid cart item data.");

        ServiceResult result = await _cartService.AddItemToCartAsync(GetUserId()!, model.ProductId, model.Quantity);
        if (result.IsBadRequest) return BadRequest(result.Errors); // TODO: Add error modal window
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

        ServiceResult<UpdatedItemQuantityViewModel> sr = await _cartService
            .UpdateItemQuantityAsync(GetUserId()!, model.ItemId, model.Quantity);
        if (sr.IsBadRequest) return BadRequest(sr.Errors);

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