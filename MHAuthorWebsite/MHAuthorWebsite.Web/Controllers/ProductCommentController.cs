using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dto;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Web.ViewModels.Product;
using MHAuthorWebsite.Web.ViewModels.ProductComment;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
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
        if (model.ParentCommentId is null)
        {
            srImages = await _imageService.UploadCommentImagesAsync(model.Images);
            if (!srImages.Success) return StatusCode(500);
        }

        ServiceResult result = await _productCommentService.AddCommentAsync(GetUserId()!, model, srImages?.Result);
        if (result.IsBadRequest) return BadRequest();
        if (!result.HasPermission) return StatusCode(403);

        return RedirectToAction(nameof(Details), "Product", new { productId = model.ProductId });
    }

    [HttpPost]
    public async Task<IActionResult> ReactToComment([FromBody] ReactToCommentViewModel model)
    {
        if (!ModelState.IsValid) return BadRequest();

        ServiceResult<ICollection<ProductCommentReactionViewModel>> sr = await _productCommentService.ReactToComment(GetUserId()!, model.CommentId, model.ReactionType);
        if (sr.IsBadRequest) return BadRequest();
        if (!sr.HasPermission) return StatusCode(403);

        return Ok(sr.Result);
    }
}