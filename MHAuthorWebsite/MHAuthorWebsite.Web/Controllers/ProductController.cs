using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Web.Utils;
using MHAuthorWebsite.Web.ViewModels.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using static MHAuthorWebsite.GCommon.ApplicationRules.Cloudinary;
using static MHAuthorWebsite.GCommon.ApplicationRules.CommentImages;
using static MHAuthorWebsite.GCommon.ApplicationRules.Pagination;
using static MHAuthorWebsite.GCommon.ApplicationRules.Roles;

namespace MHAuthorWebsite.Web.Controllers;

public class ProductController : BaseController
{
    private readonly IProductService _productService;
    private readonly IImageService _imageService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProductController(IProductService productService, IImageService imageService, UserManager<ApplicationUser> userManager)
    {
        _productService = productService;
        _imageService = imageService;
        _userManager = userManager;
    }

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

    [HttpGet]
    public async Task<IActionResult> AddComment(Guid productId, string targetName, Guid? parentCommentId)
    {
        if (parentCommentId is null && (await _userManager.GetUsersInRoleAsync(AdminRoleName)).Any(u => u.Id == GetUserId()))
            return Unauthorized();

        return View(new AddProductCommentViewModel
        {
            ProductId = productId,
            ParentCommentId = parentCommentId,
            TargetName = targetName
        });
    }

    [HttpPost]
    public async Task<IActionResult> AddComment(AddProductCommentViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        if (model.ParentCommentId is null && model.Rating is null)
        {
            ModelState.AddModelError(nameof(model.Rating), "Рейтингът е задължителен!");
            return View(model);
        }

        ServiceResult<ICollection<ImageUploadResultDto>> srImages = await _imageService.UploadImagesAsync(model.Images, CommentImagesFolder, ImageMaxWidth);
        if (!srImages.Success) return StatusCode(500);

        ServiceResult result = await _productService.AddCommentAsync(GetUserId()!, model, srImages.Result!);
        if (result.IsBadRequest) return BadRequest();
        if (!result.HasPermission) return StatusCode(403);

        return RedirectToAction(nameof(Details), new { productId = model.ProductId });
    }

    [HttpPost]
    public async Task<IActionResult> ReactToComment([FromBody] ReactToCommentViewModel model)
    {
        if (!ModelState.IsValid) return BadRequest();

        ServiceResult<ICollection<ProductCommentReactionViewModel>> sr = await _productService.ReactToComment(GetUserId()!, model.CommentId, model.ReactionType);
        if (sr.IsBadRequest) return BadRequest();
        if (!sr.HasPermission) return StatusCode(403);

        return Ok(sr.Result);
    }
}