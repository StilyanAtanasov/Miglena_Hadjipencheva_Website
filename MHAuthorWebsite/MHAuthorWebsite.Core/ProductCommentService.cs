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
using System.Text.RegularExpressions;
using static MHAuthorWebsite.GCommon.ApplicationRules.ProductComment;
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
            Text = Regex.Replace(model.Text.Trim(), @"(\r?\n\s*){2,}", "\n"),
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

    public async Task<ServiceResult<CommentPageViewModel>> LoadCommentsReadonlyAsync(Guid productId, int page, int? ratingFilter, string? userId)
    {
        Product? product = await _repository
            .AllReadonly<Product>()
            .Include(p => p.Comments)
                .ThenInclude(c => c.User)
            .Include(p => p.Comments)
                .ThenInclude(c => c.Replies)
                    .ThenInclude(c => c.User)
            .Include(p => p.Comments)
                .ThenInclude(c => c.Images)
            .Include(p => p.Comments)
                .ThenInclude(c => c.Reactions)
            .Include(p => p.Comments)
                .ThenInclude(c => c.Replies)
                    .ThenInclude(c => c.ParentReply)
                        .ThenInclude(r => r!.User)
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product is null) return ServiceResult<CommentPageViewModel>.BadRequest();
        CommentPageViewModel model = new()
        {
            HasMoreComments = product.Comments.Count(c => c.ParentCommentId == null && (!ratingFilter.HasValue || c.Rating == ratingFilter.Value)) > page * CommentPageCount,
            Comments = product.Comments
                .Where(c => c.ParentCommentId == null && (!ratingFilter.HasValue || c.Rating == ratingFilter.Value))
                .OrderBy(c => c.Reactions.Count(r => r.Reaction == CommentReaction.Like))
                .ThenByDescending(c => c.Rating)
                .ThenByDescending(c => c.Date)
                .ThenBy(c => c.Replies.Count)
                .Skip((page - 1) * CommentPageCount)
                .Take(CommentPageCount)
                .Select(c => new ProductBaseCommentViewModel
                {
                    Id = c.Id,
                    Rating = c.Rating!.Value,
                    ProductId = c.ProductId,
                    Text = c.Text,
                    UserName = userId != null && userId == c.UserId ? "Вие" : c.User.Name!,
                    Date = c.Date,
                    VerifiedPurchase = c.VerifiedPurchase,
                    Likes = c.Reactions
                        .Count(r => r.Reaction == CommentReaction.Like),
                    Dislikes = c.Reactions
                        .Count(r => r.Reaction == CommentReaction.Dislike),
                    UserReaction =
                        userId == null ? null : c.Reactions.FirstOrDefault(r => r.UserId == userId)?.Reaction,
                    ImageUrls = c.Images
                        .Select(i => i.PreviewUrl)
                        .ToArray(),
                    HasMoreReplies = c.Replies.Count > CommentRepliesPageCount,
                    TotalRepliesCount = c.Replies.Count,
                    Replies = c.Replies
                        .OrderBy(r => r.Reactions.Count(re => re.Reaction == CommentReaction.Like))
                        .Take(CommentRepliesPageCount)
                        .Select(r => new ProductCommentReplyViewModel
                        {
                            Id = r.Id,
                            Text = r.Text,
                            UserName = userId != null && userId == r.UserId ? "Вие" : r.User.Name!,
                            Date = r.Date,
                            VerifiedPurchase = r.VerifiedPurchase,
                            Likes = r.Reactions.Count(x => x.Reaction == CommentReaction.Like),
                            Dislikes = r.Reactions.Count(x => x.Reaction == CommentReaction.Dislike),
                            UserReaction = userId == null
                                ? null
                                : r.Reactions.FirstOrDefault(x => x.UserId == userId)?.Reaction,
                            IsWriterAdmin = _userManager.IsInRoleAsync(r.User, AdminRoleName).GetAwaiter().GetResult(),
                            ParentCommentId = r.ParentCommentId,
                            ProductId = r.ProductId,
                            ReplyCommentWriterName = r.ParentReply is not null
                                ? userId != null && userId == r.ParentReply!.UserId ? "Вие" : r.ParentReply!.User.Name!
                                : null
                        }).ToArray()
                }).ToArray()
        };

        return ServiceResult<CommentPageViewModel>.Ok(model);
    }

    public async Task<ServiceResult<ReplyPageViewModel>> LoadRepliesReadonlyAsync(Guid productId, Guid commentId, int page, string? userId)
    {
        ProductComment? comment = await _repository
            .WhereReadonly<ProductComment>(pc => pc.ProductId == productId && pc.Id == commentId)
            .Include(pc => pc.Replies)
                .ThenInclude(r => r.Reactions)
            .Include(pc => pc.Replies)
                .ThenInclude(pc => pc.User)
            .Include(pc => pc.Replies)
                .ThenInclude(pc => pc.ParentReply)
            .FirstOrDefaultAsync();

        if (comment is null) return ServiceResult<ReplyPageViewModel>.BadRequest();

        ReplyPageViewModel model = new()
        {
            HasMoreReplies = comment.Replies.Count > page * CommentRepliesPageCount,
            Replies = comment.Replies
                .OrderBy(r => r.Reactions.Count(re => re.Reaction == CommentReaction.Like))
                .Skip((page - 1) * CommentRepliesPageCount)
                .Take(CommentRepliesPageCount)
                .Select(r => new ProductCommentReplyViewModel
                {
                    Id = r.Id,
                    Text = r.Text,
                    UserName = userId != null && userId == r.UserId ? "Вие" : r.User.Name!,
                    Date = r.Date,
                    VerifiedPurchase = r.VerifiedPurchase,
                    Likes = r.Reactions.Count(x => x.Reaction == CommentReaction.Like),
                    Dislikes = r.Reactions.Count(x => x.Reaction == CommentReaction.Dislike),
                    UserReaction = userId == null
                        ? null
                        : r.Reactions.FirstOrDefault(x => x.UserId == userId)?.Reaction,
                    IsWriterAdmin = _userManager.IsInRoleAsync(r.User, AdminRoleName).GetAwaiter().GetResult(),
                    ParentCommentId = r.ParentCommentId,
                    ProductId = r.ProductId,
                    ReplyCommentWriterName = r.ParentReply is not null
                        ? userId != null && userId == r.ParentReply!.UserId ? "Вие" : r.ParentReply!.User.Name!
                        : null
                }).ToArray()
        };

        return ServiceResult<ReplyPageViewModel>.Ok(model);
    }
}