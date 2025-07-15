using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dto;
using MHAuthorWebsite.Data.Models.Enums;
using MHAuthorWebsite.Web.ViewModels.Product;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MHAuthorWebsite.Web.Controllers;

public class ProductController : BaseController
{
    private readonly IProductTypeService _productTypeService;
    private readonly IProductService _productService;

    public ProductController(IProductTypeService productTypeService, IProductService productService)
    {
        _productTypeService = productTypeService;
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> AddProduct()
    {
        ICollection<ProductTypeDto> productTypes = await _productTypeService.GetAllReadonlyAsync();

        ViewBag.ProductTypes = productTypes
            .Select(pt => new SelectListItem
            {
                Value = pt.Id.ToString(),
                Text = pt.Name
            })
            .ToArray();

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddProduct(AddProductForm model)
    {
        if (!ModelState.IsValid)
        {
            ICollection<ProductTypeDto> productTypes = await _productTypeService.GetAllReadonlyAsync();
            ViewBag.ProductTypes = productTypes
                .Select(pt => new SelectListItem
                {
                    Value = pt.Id.ToString(),
                    Text = pt.Name
                })
                .ToArray();
            return View(model);
        }

        ServiceResult result = await _productService.AddProductAsync(model);
        if (!result.Success) return StatusCode(500);

        return RedirectToAction(nameof(ProductsList));
    }

    [HttpGet("Product/Details/{productId}")]
    public async Task<IActionResult> Details(Guid productId)
    {
        ServiceResult<ProductDetailsViewModel> result = await _productService.GetProductDetailsReadonlyAsync(productId, GetUserId());
        if (!result.Found) return NotFound();
        if (!result.Success) return StatusCode(500);

        return View(result.Result);
    }

    [HttpGet]
    public async Task<IActionResult> AllProducts()
    {
        ICollection<ProductCardViewModel> products = await _productService.GetAllProductCardsReadonlyAsync(GetUserId());
        return View(products);
    }

    [HttpGet]
    public async Task<IActionResult> ProductsList()
    {
        ICollection<ProductListViewModel> products = await _productService.GetProductsListReadonlyAsync();
        return View(products);
    }

    [HttpGet("Product/GetCategoryTypeAttributes/{productTypeId}")]
    public async Task<IActionResult> GetCategoryTypeAttributes([FromRoute] int productTypeId)
    {
        if (HttpContext.Request.Headers["X-Requested-With"] != "XMLHttpRequest") return Forbid();

        ICollection<ProductTypeAttributesDto> attributesDto = await _productService.GetProductTypeAttributesAsync(productTypeId);

        ICollection<AttributeValueForm> attributes = attributesDto
            .Select(a => new AttributeValueForm
            {
                Key = a.Key,
                Label = a.Label,
                DataType = (AttributeDataType)a.DataType,
                IsRequired = a.IsRequired,
                HasPredefinedValue = a.HasPredefinedValue
            }).ToList();

        return PartialView("_DynamicAttributesPartial", attributes);
    }

    [HttpPost("/Product/DeleteProduct/{productId}")]
    public async Task<IActionResult> DeleteProduct([FromRoute] Guid productId)
    {
        ServiceResult result = await _productService.DeleteProductAsync(productId);
        if (!result.Found) return NotFound();
        if (!result.Success) return StatusCode(500);

        return RedirectToAction(nameof(ProductsList));
    }

    [HttpPost("/Product/TogglePublicity/{productId}")]
    public async Task<IActionResult> TogglePublicity([FromRoute] Guid productId)
    {
        ServiceResult result = await _productService.ToggleProductPublicityAsync(productId);
        if (!result.Found) return NotFound();
        if (!result.Success) return StatusCode(500);

        return RedirectToAction(nameof(ProductsList));
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