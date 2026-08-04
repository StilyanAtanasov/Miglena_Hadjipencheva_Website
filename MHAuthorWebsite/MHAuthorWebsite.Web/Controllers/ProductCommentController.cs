using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dtos.ProductComment;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Web.Utils.Extensions;
using MHAuthorWebsite.Web.ViewModels.ProductComment;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static MHAuthorWebsite.GCommon.ApplicationRules.Roles;
using static MHAuthorWebsite.Web.Utils.Mappers.ImageMapper;

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
        ServiceResult<ProductCommentDetailsDto> sr =
            await _productCommentService.GetCommentDetailsReadonlyAsync(commentId, GetUserId());
        if (!sr.Found) return NotFound();

        ProductCommentDetailsDto dto = sr.Result!;

        ProductCommentDetailsViewModel viewModel = new ProductCommentDetailsViewModel
        {
            Id = dto.Id,
            Text = dto.Text,
            UserReaction = dto.UserReaction,
            Rating = dto.Rating,
            Date = dto.Date,
            Dislikes = dto.Dislikes,
            Likes = dto.Likes,
            Images = dto.Images
                .Select(i => new ProductCommentImageViewModel
                {
                    ImageUrl = i.ImageUrl,
                    ImagePreviewUrl = i.ImagePreviewUrl,
                })
                .ToArray(),
            ProductId = dto.ProductId,
            UserName = dto.UserName,
            VerifiedPurchase = dto.VerifiedPurchase
        };

        return PartialView("_ProductCommentDetails", viewModel);
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
            if (model.Images.ContainsImageExceedingCloudinarySizeLimit())
            {
                ModelState.AddModelError(nameof(model.Images), ImageValidationExtensions.GetCloudinarySizeLimitValidationMessage());
                return View(model);
            }

            srImages = await _imageService.UploadCommentImagesAsync(
                await MapIFormFileCollectionToUploadImageRequestDtoAsync(model.Images, HttpContext.RequestAborted),
                HttpContext.RequestAborted);
            if (!srImages.Success) return StatusCode(500);
        }

        AddProductCommentDto addProductCommentDto = new AddProductCommentDto
        {
            ProductId = model.ProductId,
            ParentCommentId = model.ParentCommentId,
            ReplyCommentId = model.ReplyCommentId,
            Text = model.Text,
            Rating = model.Rating,
            TargetName = model.TargetName
        };


        ServiceResult sr = await _productCommentService.AddCommentAsync(GetUserId()!, addProductCommentDto, srImages?.Result);
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
        ServiceResult<EditProductCommentDto> sr = await _productCommentService.GetCommentForEditReadonlyAsync(GetUserId()!, commentId);
        if (sr.IsBadRequest) return BadRequest();
        if (!sr.HasPermission) return StatusCode(403);

        EditProductCommentDto dto = sr.Result!;

        EditProductCommentViewModel viewModel = new EditProductCommentViewModel
        {
            CommentId = dto.CommentId,
            ProductId = dto.ProductId,
            Text = dto.Text,
            Rating = dto.Rating,
            ParentCommentId = dto.ParentCommentId,
            ReplyCommentId = dto.ReplyCommentId,
            RemovedImagesUrls = dto.RemovedImagesUrls,
            ImagePreviewUrls = dto.ImagePreviewUrls
                .Select(i => new EditProductCommentImageViewModel
                {
                    PreviewUrl = i.PreviewUrl,
                    ImageId = i.ImageId,
                })
                .ToArray()
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EditProductCommentViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        ServiceResult<ICollection<ProductCommentImagesUploadDto>>? uploadSr = null;
        if (model.NewImages is not null && model.NewImages.Count > 0)
        {
            if (model.NewImages.ContainsImageExceedingCloudinarySizeLimit())
            {
                ModelState.AddModelError(nameof(model.NewImages), ImageValidationExtensions.GetCloudinarySizeLimitValidationMessage());
                return View(model);
            }

            uploadSr = await _imageService.UploadCommentImagesAsync(
                await MapIFormFileCollectionToUploadImageRequestDtoAsync(model.NewImages, HttpContext.RequestAborted),
                HttpContext.RequestAborted);
        }
        if (uploadSr is not null && !uploadSr.Success) return StatusCode(500);

        EditProductCommentDto dto = new EditProductCommentDto
        {
            CommentId = model.CommentId,
            Text = model.Text,
            Rating = model.Rating,
            ParentCommentId = model.ParentCommentId,
            ReplyCommentId = model.ReplyCommentId,
            ImagePreviewUrls = model.ImagePreviewUrls
                .Select(i => new EditProductCommentImageDto
                {
                    PreviewUrl = i.PreviewUrl,
                    ImageId = i.ImageId,
                })
                .ToArray(),
            RemovedImagesUrls = model.RemovedImagesUrls,
            ProductId = model.ProductId
        };

        ServiceResult<ICollection<string>> sr = await _productCommentService.EditCommentAsync(GetUserId()!, dto, uploadSr?.Result, model.RemovedImagesUrls?.Select(Guid.Parse).ToArray());
        if (sr.IsBadRequest) return BadRequest();
        if (!sr.HasPermission) return StatusCode(403);

        ServiceResult? deleteImagesSr =
            sr.Result is not null && sr.Result.Count > 0
            ? await _imageService.DeleteCommentImagesAsync(sr.Result!, HttpContext.RequestAborted)
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

        ServiceResult<ICollection<ProductCommentReactionDto>> sr = await _productCommentService.ReactToComment(GetUserId()!, model.CommentId, model.ReactionType);
        if (sr.IsBadRequest) return BadRequest();
        if (!sr.HasPermission) return StatusCode(403);

        return Ok(sr.Result);
    }

    [HttpGet]
    public async Task<IActionResult> LoadComments(Guid productId, int page, int? ratingFilter)
    {
        ServiceResult<CommentPageDto> sr = await _productCommentService.LoadCommentsReadonlyAsync(productId, page, ratingFilter, GetUserId());
        if (sr.IsBadRequest) return BadRequest();

        ICollection<ProductBaseCommentViewModel> vm = sr.Result!.Comments
            .Select(c => new ProductBaseCommentViewModel
            {
                Id = c.Id,
                Text = c.Text,
                UserReaction = c.UserReaction,
                Rating = c.Rating,
                Date = c.Date,
                Likes = c.Likes,
                Dislikes = c.Dislikes,
                UserName = c.UserName,
                VerifiedPurchase = c.VerifiedPurchase,
                HasMoreReplies = c.HasMoreReplies,
                ImageUrls = c.ImageUrls,
                IsUserAuthor = c.IsUserAuthor,
                TotalRepliesCount = c.TotalRepliesCount,
                LastEdited = c.LastEdited,
                ProductId = c.ProductId,
                Replies = c.Replies
                    .Select(r => new ProductCommentReplyViewModel
                    {
                        Id = r.Id,
                        Text = r.Text,
                        UserReaction = r.UserReaction,
                        Date = r.Date,
                        Likes = r.Likes,
                        Dislikes = r.Dislikes,
                        UserName = r.UserName,
                        IsUserAuthor = r.IsUserAuthor,
                        LastEdited = r.LastEdited,
                        ProductId = r.ProductId,
                        IsWriterAdmin = r.IsWriterAdmin,
                        ParentCommentId = r.ParentCommentId,
                        ReplyCommentWriterName = r.ReplyCommentWriterName,
                        VerifiedPurchase = r.VerifiedPurchase
                    })
                    .ToArray()
            })
            .ToArray();

        bool hasMore = sr.Result!.HasMoreComments;
        string html = await this.RenderViewAsync(
            "_ProductComments",
            vm,
            partial: true
        );

        return Json(new { Comments = html, HasMoreComments = hasMore });
    }

    [HttpGet]
    public async Task<IActionResult> LoadReplies(Guid productId, Guid commentId, int page)
    {
        ServiceResult<ReplyPageDto> sr = await _productCommentService.LoadRepliesReadonlyAsync(productId, commentId, page, GetUserId());
        if (sr.IsBadRequest) return BadRequest();

        ICollection<ProductCommentReplyViewModel> vm = sr.Result!.Replies
            .Select(r => new ProductCommentReplyViewModel
            {
                Id = r.Id,
                Text = r.Text,
                UserReaction = r.UserReaction,
                Date = r.Date,
                Likes = r.Likes,
                Dislikes = r.Dislikes,
                UserName = r.UserName,
                IsUserAuthor = r.IsUserAuthor,
                LastEdited = r.LastEdited,
                ProductId = r.ProductId,
                IsWriterAdmin = r.IsWriterAdmin,
                ParentCommentId = r.ParentCommentId,
                ReplyCommentWriterName = r.ReplyCommentWriterName,
                VerifiedPurchase = r.VerifiedPurchase
            })
            .ToArray();

        bool hasMore = sr.Result!.HasMoreReplies;
        string html = await this.RenderViewAsync(
            "_CommentReplies",
            vm,
            partial: true
        );

        return Json(new { Replies = html, HasMoreReplies = hasMore });
    }
}
