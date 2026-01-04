using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dtos.Product;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Web.Utils;
using MHAuthorWebsite.Web.Utils.Attributes;
using MHAuthorWebsite.Web.Utils.Enums;
using MHAuthorWebsite.Web.ViewModels.Product;
using MHAuthorWebsite.Web.ViewModels.ProductComment;
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
    [SecurityHeaders(CspFeature.Notifications | CspFeature.Editor)]
    public async Task<IActionResult> Details(Guid productId)
    {
        ServiceResult<ProductDetailsDto> result = await _productService.GetProductDetailsReadonlyAsync(productId, GetUserId());
        if (!result.Found) return NotFound();
        if (!result.Success) return StatusCode(500);

        ProductDetailsDto dto = result.Result!;

        ProductDetailsViewModel viewModel = new ProductDetailsViewModel
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            IsLiked = dto.IsLiked,
            Price = dto.Price,
            Discount = dto.Discount != null ? new ProductDetailsDiscountViewModel
            {
                EndDate = dto.Discount.EndDate,
                NewPrice = dto.Discount.NewPrice
            } : null,
            IsInStock = dto.IsInStock,
            ProductTypeName = dto.ProductTypeName,
            TotalBaseComments = dto.TotalBaseComments,
            AverageRating = dto.AverageRating,
            CommentsCountByStarsRating = dto.CommentsCountByStarsRating,
            HasMoreComments = dto.HasMoreComments,
            CanWriteMoreComments = dto.CanWriteMoreComments,
            IsRateLimitedForReplies = dto.IsRateLimitedForReplies,
            Images = dto.Images
                .Select(i => new ProductDetailsImageViewModel
                {
                    ImageUrl = i.ImageUrl,
                    AltText = i.AltText,
                })
                .ToArray(),
            Attributes = dto.Attributes
                .Select(a => new ProductAttributeDetailsViewModel
                {
                    Label = a.Label,
                    Value = a.Value,
                    DisplayPosition = a.DisplayPosition
                })
                .ToArray(),
            Comments = dto.Comments
                .Select(c => new ProductBaseCommentViewModel
                {
                    Id = c.Id,
                    UserName = c.UserName,
                    VerifiedPurchase = c.VerifiedPurchase,
                    Text = c.Text,
                    Rating = c.Rating,
                    Date = c.Date,
                    Likes = c.Likes,
                    Dislikes = c.Dislikes,
                    UserReaction = c.UserReaction,
                    HasMoreReplies = c.HasMoreReplies,
                    TotalRepliesCount = c.TotalRepliesCount,
                    IsUserAuthor = c.IsUserAuthor,
                    LastEdited = c.LastEdited,
                    ProductId = c.ProductId,
                    ImageUrls = c.ImageUrls,
                    Replies = c.Replies
                        .Select(r => new ProductCommentReplyViewModel
                        {
                            Id = r.Id,
                            UserName = r.UserName,
                            Text = r.Text,
                            Date = r.Date,
                            Likes = r.Likes,
                            Dislikes = r.Dislikes,
                            IsUserAuthor = r.IsUserAuthor,
                            LastEdited = r.LastEdited,
                            ProductId = r.ProductId,
                            IsWriterAdmin = r.IsWriterAdmin,
                            ParentCommentId = r.ParentCommentId,
                            UserReaction = r.UserReaction,
                            ReplyCommentWriterName = r.ReplyCommentWriterName,
                            VerifiedPurchase = r.VerifiedPurchase
                        })
                        .ToArray(),
                })
                .ToArray()
        };

        return View(viewModel);
    }

    [SecurityHeaders(CspFeature.TomSelect | CspFeature.Notifications)]
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
        ICollection<ProductCardDto> products = await _productService.GetAllProductCardsReadonlyAsync(GetUserId(), page, sortType);

        ViewBag.ProductsCount = productsCount;

        ICollection<ProductCardViewModel> productsViewModel = products
            .Select(p => new ProductCardViewModel
            {
                Id = p.Id,
                Name = p.Name,
                ImageUrl = p.ImageUrl,
                ImageAlt = p.ImageAlt,
                Price = p.Price,
                DiscountPrice = p.DiscountPrice,
                IsAvailable = p.IsAvailable,
                IsLiked = p.IsLiked,
                ProductType = p.ProductType
            })
            .ToArray();

        return View(productsViewModel);
    }

    [HttpGet]
    [SecurityHeaders(CspFeature.Notifications)]
    public async Task<IActionResult> LikedProducts()
    {
        string? userId = GetUserId();
        if (userId is null) return Unauthorized();

        ICollection<LikedProductDto> productsDto = await _productService.GetLikedProductsReadonlyAsync(userId);

        ICollection<LikedProductViewModel> viewModel = productsDto
            .Select(p => new LikedProductViewModel
            {
                Id = p.Id,
                Name = p.Name,
                ThumbnailUrl = p.ThumbnailUrl,
                ThumbnailAlt = p.ThumbnailAlt,
                Price = p.Price,
                CategoryName = p.CategoryName,
                IsInStock = p.IsInStock,
                DiscountedPrice = p.DiscountedPrice
            })
            .ToArray();

        return View(viewModel);
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