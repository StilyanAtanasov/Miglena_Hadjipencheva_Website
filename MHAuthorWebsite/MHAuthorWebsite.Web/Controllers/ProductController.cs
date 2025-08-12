using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Web.Utils;
using MHAuthorWebsite.Web.ViewModels.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using static MHAuthorWebsite.GCommon.ApplicationRules.Pagination;

namespace MHAuthorWebsite.Web.Controllers;

public class ProductController : BaseController
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService) => _productService = productService;

    [AllowAnonymous]
    [HttpGet("Product/Details/{productId}")]
    public async Task<IActionResult> Details(Guid productId)
    {
        ServiceResult<ProductDetailsViewModel> result = await _productService.GetProductDetailsReadonlyAsync(productId, GetUserId());
        if (!result.Found) return NotFound();
        if (!result.Success) return StatusCode(500);

        return View(result.Result);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> AllProducts([FromQuery] int page = 1, [FromQuery] string? orderType = null)
    {
        if (page < 1) page = 1;
        if (orderType is null) return RedirectToAction(nameof(AllProducts), new { page, orderType = "recommended" });

        bool result = SortValueMapper.SortMap.TryGetValue(orderType, out var sortValue);
        if (!result) return RedirectToAction(nameof(AllProducts), new { page, orderType = "recommended" });

        int productsCount = await _productService.GetAllProductsCountAsync();
        if (productsCount > 0 && Math.Ceiling((double)productsCount / PageSize) < page) return NotFound();

        (bool descending, Expression<Func<Product, object>>? expression) sortType = (sortValue.descending, sortValue.expression);
        ICollection<ProductCardViewModel> products = await _productService.GetAllProductCardsReadonlyAsync(GetUserId(), page, sortType);


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

    [AllowAnonymous]
    [HttpPost("/Product/ToggleLike/{productId}")]
    public async Task<IActionResult> ToggleLike([FromRoute] Guid productId)
    {
        string? userId = GetUserId();
        if (userId is null) return Unauthorized();

        ServiceResult result = await _productService.ToggleLikeProduct(userId, productId);
        if (!result.Found) return NotFound();
        if (!result.HasPermission) return Unauthorized();

        return StatusCode(200);
    }
}