using CloudinaryDotNet.Actions;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dto;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Data.Models.Enums;
using MHAuthorWebsite.Data.Shared;
using MHAuthorWebsite.Web.ViewModels.Product;
using MHAuthorWebsite.Web.ViewModels.ProductComment;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static MHAuthorWebsite.GCommon.ApplicationRules.Roles;

namespace MHAuthorWebsite.Core;

public class ProductCommentService : IProductCommentService
{
    private readonly IApplicationRepository _repository;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProductCommentService(IApplicationRepository repository, UserManager<ApplicationUser> userManager)
    {
        _repository = repository;
        _userManager = userManager;
    }
    public async Task<ServiceResult> AddCommentAsync(string userId, AddProductCommentViewModel model, ICollection<ProductCommentImagesUploadDto>? images)
    {
        if (images is null && model.ParentCommentId is null) return ServiceResult.BadRequest();

        Product? product = await _repository
            .All<Product>()
            .Include(p => p.Orders)
                .ThenInclude(op => op.Order)
            .FirstOrDefaultAsync(p => p.Id == model.ProductId);
        if (product is null) return ServiceResult.BadRequest();

        ApplicationUser? user = await _userManager.FindByIdAsync(userId);
        if (user is null || (await _userManager.IsInRoleAsync(user, AdminRoleName) && model.ParentCommentId is null)) return ServiceResult.Forbidden();

        ProductComment? parentComment = model.ParentCommentId != null
            ? await _repository.All<ProductComment>().FirstOrDefaultAsync(c => c.Id == model.ParentCommentId)
            : null;

        if (model.ParentCommentId is not null && parentComment is null) return ServiceResult.BadRequest();

        /* if (product.Comments.Any(c => c.UserId == userId && c.ParentCommentId == null))
             return ServiceResult.BadRequest();
 */
        // TODO Add validation for parent comment and for max comments per product per user

        product.Comments.Add(new ProductComment
        {
            UserId = userId,
            ParentCommentId = model.ParentCommentId,
            Rating = model.Rating,
            Text = model.Text,
            VerifiedPurchase = product.Orders.Any(o => o.Order.UserId == userId), // TODO confirm order is received
            Date = DateTime.UtcNow,
            Images = images is not null ? images.Select(i => new ProductCommentImage
            {
                ImageUrl = i.Image.ImageUrl,
                PublicId = i.Image.PublicId,
                AltText = model.TargetName,
                PreviewUrl = i.Preview.ImageUrl,
                PreviewPublicId = i.Preview.PublicId
            }).ToList() : new HashSet<ProductCommentImage>(),
            ParentReplyId = model.ReplyCommentId,
            ProductId = model.ProductId
        });

        await _repository.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult<ICollection<ProductCommentReactionViewModel>>> ReactToComment(string userId, Guid commentId, CommentReaction reactionType)
    {
        ProductComment? comment = await _repository
            .All<ProductComment>()
            .Include(c => c.Reactions)
            .FirstOrDefaultAsync(c => c.Id == commentId);
        if (comment is null) return ServiceResult<ICollection<ProductCommentReactionViewModel>>.BadRequest();

        bool isValidReaction = Enum.IsDefined(typeof(CommentReaction), reactionType);
        if (!isValidReaction) return ServiceResult<ICollection<ProductCommentReactionViewModel>>.BadRequest();

        if (comment.UserId == userId) return ServiceResult<ICollection<ProductCommentReactionViewModel>>.Forbidden();

        if (comment.Reactions.All(r => r.UserId != userId))
        {
            comment.Reactions.Add(new ProductCommentReaction
            {
                UserId = userId,
                Reaction = reactionType,
                CommentId = commentId,
                CreatedAt = DateTime.Now
            });
        }
        else
        {
            ProductCommentReaction existingReaction = comment.Reactions.First(r => r.UserId == userId);
            if (existingReaction.Reaction == reactionType) comment.Reactions.Remove(existingReaction);
            else
            {
                existingReaction.Reaction = reactionType;
                existingReaction.CreatedAt = DateTime.Now;
            }
        }

        await _repository.SaveChangesAsync();

        IEnumerable<CommentReaction> allReactions = Enum.GetValues(typeof(CommentReaction))
            .Cast<CommentReaction>();

        ICollection<ProductCommentReactionViewModel> reactions = allReactions
            .Select(r => new ProductCommentReactionViewModel
            {
                Reaction = (int)r,
                Count = comment.Reactions.Count(x => x.Reaction == r)
            })
            .ToArray();

        return ServiceResult<ICollection<ProductCommentReactionViewModel>>.Ok(reactions);
    }
}