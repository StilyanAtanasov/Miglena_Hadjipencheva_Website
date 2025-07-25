﻿using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Web.ViewModels.Cart;
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
    public async Task<IActionResult> Add([FromForm] AddCartItemViewModel model)
    {
        if (!IsUserAuthenticated()) return Unauthorized();
        if (model.ProductId == Guid.Empty || model.Quantity <= 0) return BadRequest("Invalid cart item data.");

        ServiceResult result = await _cartService.AddItemToCartAsync(GetUserId()!, model.ProductId, model.Quantity);
        if (result.IsBadRequest) return BadRequest(result.Errors); // TODO: Add error modal window
        if (!result.Success) return StatusCode(500);

        string referer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referer)) return Redirect(referer);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Cart/Remove/{itemId}")]
    public async Task<IActionResult> Remove([FromRoute] Guid itemId)
    {
        if (!IsUserAuthenticated()) return Unauthorized();

        ServiceResult r = await _cartService.RemoveFromCartAsync(GetUserId()!, itemId);
        if (r.IsBadRequest) return BadRequest(r.Errors);

        return RedirectToAction(nameof(Index));
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
}