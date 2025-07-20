using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Web.Utils;
using MHAuthorWebsite.Web.ViewModels.Product;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace MHAuthorWebsite.Web.Controllers;

public class ProductController : BaseController
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService) => _productService = productService;

    [HttpGet("Product/Details/{productId}")]
    public async Task<IActionResult> Details(Guid productId)
    {
        ServiceResult<ProductDetailsViewModel> result = await _productService.GetProductDetailsReadonlyAsync(productId, GetUserId());
        if (!result.Found) return NotFound();
        if (!result.Success) return StatusCode(500);

        return View(result.Result);
    }

    [HttpGet]
    public async Task<IActionResult> AllProducts([FromQuery] int page = 1, [FromQuery] string? orderType = null)
    {
        if (page < 1) page = 1;
        orderType ??= "recommended";

        (bool descending, Expression<Func<Product, object>>? expression) sortType = (false, null);

        bool result = SortValueMapper.SortMap.TryGetValue(orderType, out var sortValue);
        if (result)
        {
            sortType.descending = sortValue.descending;
            sortType.expression = sortValue.expression;
        }

        ICollection<ProductCardViewModel> products = await _productService.GetAllProductCardsReadonlyAsync(GetUserId(), page, sortType);
        int productsCount = await _productService.GetAllProductsCountAsync();
        if (!products.Any()) return NotFound();

        ViewBag.ProductsCount = productsCount;

        return View(products);
    }

    [HttpGet]
    public async Task<IActionResult> LikedProducts()
    {
        string? userId = GetUserId();
        if (userId is null) return Unauthorized();

        ICollection<LikedProductViewModel> products = await _productService.GetLikedProductsReadonlyAsync(userId);
        return View(products);
    }

    [HttpPost("/Product/ToggleLike/{productId}")]
    public async Task<IActionResult> ToggleLike([FromRoute] Guid productId)
    {
        string? userId = GetUserId();
        if (userId is null) return Unauthorized();

        ServiceResult result = await _productService.ToggleLikeProduct(userId, productId);
        if (!result.Found) return NotFound();
        if (!result.HasPermission) return Unauthorized();

        string referer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referer)) return Redirect(referer);

        return RedirectToAction(nameof(Index), "Home");
    }
}