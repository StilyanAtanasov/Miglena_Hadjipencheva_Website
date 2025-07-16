using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Web.ViewModels.Cart;
using Microsoft.AspNetCore.Mvc;

namespace MHAuthorWebsite.Web.Controllers;

public class CartController : BaseController
{
    private readonly ICartService _cartService;
    public CartController(ICartService cartService) => _cartService = cartService;

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

        return RedirectToAction(nameof(GetCart));
    }

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        throw new NotImplementedException();
    }
}