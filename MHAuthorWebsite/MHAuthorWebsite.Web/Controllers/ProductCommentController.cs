using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dto;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Web.Utils.Extensions;
using MHAuthorWebsite.Web.ViewModels.ProductComment;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static MHAuthorWebsite.GCommon.ApplicationRules.Roles;

namespace MHAuthorWebsite.Web.Controllers;

public class ProductCommentController : BaseController
{
    private readonly IProductCommentService _productCommentService;
    private readonly ICommentImageService _imageService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProductCommentController(IProductCommentService productCommentService, ICommentImageService imageService, UserManager<ApplicationUser> userManager)
    {
        _productCommentService = productCommentService;
        _imageService = imageService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid commentId)
    {
        ServiceResult<ProductCommentDetailsViewModel> sr =
            await _productCommentService.GetCommentDetailsReadonlyAsync(commentId, GetUserId());
        if (!sr.Found) return NotFound();

        return PartialView("_ProductCommentDetails", sr.Result);
    }

    [HttpGet]
    public async Task<IActionResult> AddComment(Guid productId, string targetName, Guid? parentCommentId, Guid? replyCommentId)
    {
        if (parentCommentId is null && (await _userManager.GetUsersInRoleAsync(AdminRoleName)).Any(u => u.Id == GetUserId()))
            return Unauthorized();

        return View(new AddProductCommentViewModel
        {
            ProductId = productId,
            ParentCommentId = parentCommentId,
            ReplyCommentId = replyCommentId,
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

        ServiceResult<ICollection<ProductCommentImagesUploadDto>>? srImages = null;
        if (model.ParentCommentId is null && model.Images is not null)
        {
            srImages = await _imageService.UploadCommentImagesAsync(model.Images);
            if (!srImages.Success) return StatusCode(500);
        }

        ServiceResult sr = await _productCommentService.AddCommentAsync(GetUserId()!, model, srImages?.Result);
        if (!sr.Success)
        {
            if (sr.Errors.TryGetValue("Limit", out var limitError)) return StatusCode(500, limitError);
            if (sr.Errors.TryGetValue("RateLimit", out var rateLimitError)) return StatusCode(429, rateLimitError);
            if (sr.IsBadRequest) return BadRequest();
            if (!sr.HasPermission) return StatusCode(403);
        }

        return RedirectToAction(nameof(Details), "Product", new { productId = model.ProductId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid commentId)
    {
        ServiceResult<EditProductCommentViewModel> sr = await _productCommentService.GetCommentForEditReadonlyAsync(GetUserId()!, commentId);
        if (sr.IsBadRequest) return BadRequest();
        if (!sr.HasPermission) return StatusCode(403);

        return View(sr.Result);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EditProductCommentViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        ServiceResult<ICollection<ProductCommentImagesUploadDto>>? uploadSr =
            model.NewImages is not null && model.NewImages.Count > 0
            ? await _imageService.UploadCommentImagesAsync(model.NewImages)
            : null;
        if (uploadSr is not null && !uploadSr.Success) return StatusCode(500);

        ServiceResult<ICollection<string>> sr = await _productCommentService.EditCommentAsync(GetUserId()!, model, uploadSr?.Result, model.RemovedImagesUrls?.Select(Guid.Parse).ToArray());
        if (sr.IsBadRequest) return BadRequest();
        if (!sr.HasPermission) return StatusCode(403);

        ServiceResult? deleteImagesSr =
            sr.Result is not null && sr.Result.Count > 0
            ? await _imageService.DeleteCommentImagesAsync(sr.Result!)
            : null;
        if (deleteImagesSr is not null && !deleteImagesSr.Success) return StatusCode(500);

        return RedirectToAction(nameof(Details), "Product", new { productId = model.ProductId });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid commentId)
    {
        ServiceResult sr = await _productCommentService.DeleteCommentAsync(GetUserId()!, commentId);
        if (sr.IsBadRequest) return BadRequest();
        if (!sr.HasPermission) return StatusCode(403);

        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetAverageRating(Guid productId)
        => Ok(await _productCommentService.GetAverageRatingAsync(productId));

    [HttpPost]
    public async Task<IActionResult> ReactToComment([FromBody] ReactToCommentViewModel model)
    {
        if (!ModelState.IsValid) return BadRequest();

        ServiceResult<ICollection<ProductCommentReactionViewModel>> sr = await _productCommentService.ReactToComment(GetUserId()!, model.CommentId, model.ReactionType);
        if (sr.IsBadRequest) return BadRequest();
        if (!sr.HasPermission) return StatusCode(403);

        return Ok(sr.Result);
    }

    [HttpGet]
    public async Task<IActionResult> LoadComments(Guid productId, int page, int? ratingFilter)
    {
        ServiceResult<CommentPageViewModel> sr = await _productCommentService.LoadCommentsReadonlyAsync(productId, page, ratingFilter, GetUserId());
        if (sr.IsBadRequest) return BadRequest();

        bool hasMore = sr.Result!.HasMoreComments;
        string html = await this.RenderViewAsync(
            "_ProductComments",
            sr.Result!.Comments,
            partial: true
        );

        return Json(new { Comments = html, HasMoreComments = hasMore });
    }

    [HttpGet]
    public async Task<IActionResult> LoadReplies(Guid productId, Guid commentId, int page)
    {
        ServiceResult<ReplyPageViewModel> sr = await _productCommentService.LoadRepliesReadonlyAsync(productId, commentId, page, GetUserId());
        if (sr.IsBadRequest) return BadRequest();

        bool hasMore = sr.Result!.HasMoreReplies;
        string html = await this.RenderViewAsync(
            "_CommentReplies",
            sr.Result!.Replies,
            partial: true
        );

        return Json(new { Replies = html, HasMoreReplies = hasMore });
    }
}