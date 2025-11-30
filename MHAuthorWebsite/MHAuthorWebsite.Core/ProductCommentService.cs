using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dto;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Data.Models.Enums;
using MHAuthorWebsite.Data.Shared;
using MHAuthorWebsite.Web.ViewModels.ProductComment;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using static MHAuthorWebsite.GCommon.ApplicationRules.CacheKeys;
using static MHAuthorWebsite.GCommon.ApplicationRules.ProductComment;
using static MHAuthorWebsite.GCommon.ApplicationRules.Roles;

namespace MHAuthorWebsite.Core;

public class ProductCommentService : IProductCommentService
{
    private readonly IFastCacheService _cache;
    private readonly IApplicationRepository _repository;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProductCommentService(IFastCacheService cache, IApplicationRepository repository, UserManager<ApplicationUser> userManager)
    {
        _cache = cache;
        _repository = repository;
        _userManager = userManager;
    }

    public async Task<ServiceResult<ProductCommentDetailsViewModel>> GetCommentDetailsReadonlyAsync(Guid commentId, string? userId)
    {
        ProductCommentDetailsViewModel? comment = await _repository
            .AllReadonly<ProductComment>()
            .Where(c => c.Id == commentId)
            .Include(c => c.User)
            .Include(c => c.Reactions)
            .Include(c => c.Images)
            .Select(c => new ProductCommentDetailsViewModel
            {
                Id = c.Id,
                ProductId = c.ProductId,
                Rating = c.Rating!.Value,
                Text = c.Text,
                UserName = c.User.Name!,
                Date = c.Date,
                VerifiedPurchase = c.VerifiedPurchase,
                Likes = c.Reactions.Count(r => r.Reaction == CommentReaction.Like),
                Dislikes = c.Reactions.Count(r => r.Reaction == CommentReaction.Dislike),
                Images = c.Images
                    .Select(i => new ProductCommentImageViewModel
                    {
                        ImageUrl = i.ImageUrl,
                        ImagePreviewUrl = i.PreviewUrl
                    })
                    .ToArray(),
                UserReaction =
                    userId == null ? null : c.Reactions
                        .Where(r => r.UserId == userId)
                        .Select(r => (CommentReaction?)r.Reaction)
                        .FirstOrDefault()
            })
            .FirstOrDefaultAsync();

        return comment is null
            ? ServiceResult<ProductCommentDetailsViewModel>.NotFound()
            : ServiceResult<ProductCommentDetailsViewModel>.Ok(comment);
    }

    public async Task<ServiceResult> AddCommentAsync(string userId, AddProductCommentViewModel model, ICollection<ProductCommentImagesUploadDto>? images)
    {
        Product? product = await _repository
            .All<Product>()
            .Include(p => p.Orders)
                .ThenInclude(op => op.Order)
            .Include(p => p.Comments)
            .FirstOrDefaultAsync(p => p.Id == model.ProductId);
        if (product is null) return ServiceResult.BadRequest();

        ApplicationUser? user = await _userManager.FindByIdAsync(userId);
        if (user is null || (await _userManager.IsInRoleAsync(user, AdminRoleName) && model.ParentCommentId is null)) return ServiceResult.Forbidden();

        ProductComment? parentComment = model.ParentCommentId != null
            ? await _repository.All<ProductComment>().FirstOrDefaultAsync(c => c.Id == model.ParentCommentId)
            : null;

        if (model.ParentCommentId is not null && parentComment is null) return ServiceResult.BadRequest();

        if (product.Comments.Any(c => c.UserId == userId && c.ParentCommentId == null) && model.ParentCommentId == null)
            return ServiceResult.Failure(new() { ["Limit"] = "Всеки потребител има право на един базов коментар за продукт!" });

        if (product.Comments
                .Count(c => c.UserId == userId && c.ParentCommentId != null
                                               && c.Date > DateTime.UtcNow
                                                   .AddHours(-MaxRepliesTimeFrameHours)) > MaxRepliesForTimeFrame
             && model.ParentCommentId != null)
            return ServiceResult.Failure(new()
            {
                ["RateLimit"] = "Вие добавихте прекалено много отговори за кратък период. Моля, опитайте пак по-късно!"
            });

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

        await _cache.RemoveAsync(ProductCommentsKey(model.ProductId));
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult<EditProductCommentViewModel>> GetCommentForEditReadonlyAsync(string userId, Guid commentId)
    {
        ProductComment? comment = await _repository
            .All<ProductComment>()
            .Include(c => c.Images)
            .FirstOrDefaultAsync(c => c.Id == commentId && c.UserId == userId);

        if (comment is null) return ServiceResult<EditProductCommentViewModel>.NotFound();
        if (comment.UserId != userId) return ServiceResult<EditProductCommentViewModel>.BadRequest();

        EditProductCommentViewModel model = new()
        {
            CommentId = comment.Id,
            ProductId = comment.ProductId,
            ParentCommentId = comment.ParentCommentId,
            ReplyCommentId = comment.ParentReplyId,
            Rating = comment.Rating,
            Text = comment.Text,
            ImagePreviewUrls = comment.Images.Select(i => new EditProductCommentImageViewModel()
            {
                ImageId = i.Id,
                PreviewUrl = i.PreviewUrl,
            }).ToArray()
        };

        return ServiceResult<EditProductCommentViewModel>.Ok(model);
    }

    public async Task<ServiceResult<ICollection<string>>> EditCommentAsync(string userId, EditProductCommentViewModel model, ICollection<ProductCommentImagesUploadDto>? newImages, ICollection<Guid>? removedImagesUrls)
    {
        ProductComment? comment = await _repository
            .All<ProductComment>()
            .Include(c => c.Images)
            .FirstOrDefaultAsync(c => c.Id == model.CommentId && c.UserId == userId);

        if (comment is null) return ServiceResult<ICollection<string>>.BadRequest();
        if (comment.UserId != userId) return ServiceResult<ICollection<string>>.BadRequest();

        comment.Rating = model.Rating;
        comment.Text = model.Text;
        comment.LastEdited = DateTime.Now;

        if (newImages is not null && newImages.Count > 0)
        {
            foreach (ProductCommentImagesUploadDto imageDto in newImages)
            {
                comment.Images.Add(new ProductCommentImage
                {
                    ImageUrl = imageDto.Image.ImageUrl,
                    PublicId = imageDto.Image.PublicId,
                    AltText = model.Text,
                    PreviewUrl = imageDto.Preview.ImageUrl,
                    PreviewPublicId = imageDto.Preview.PublicId
                });
            }
        }

        ICollection<string> publicIdsToDelete = new HashSet<string>();
        if (removedImagesUrls is not null && removedImagesUrls.Count > 0)
        {
            ICollection<ProductCommentImage> imagesToRemove = comment.Images
                .Where(i => removedImagesUrls.Contains(i.Id))
                .ToArray();

            foreach (ProductCommentImage image in imagesToRemove)
            {
                publicIdsToDelete.Add(image.PublicId);
                comment.Images.Remove(image);
            }
        }

        await _repository.SaveChangesAsync();
        await _cache.RemoveAsync(ProductCommentsKey(model.ProductId));

        return ServiceResult<ICollection<string>>.Ok(publicIdsToDelete);
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
        await _cache.RemoveAsync(ProductDetailsUserDataKey(comment.ProductId, userId));
        await _cache.RemoveAsync(ProductCommentsKey(comment.ProductId));

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
                .OrderByDescending(c => c.Reactions.Count(r => r.Reaction == CommentReaction.Like))
                .ThenByDescending(c => c.Rating)
                .ThenByDescending(c => c.Date)
                .ThenByDescending(c => c.Replies.Count)
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
                    LastEdited = c.LastEdited,
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
                    IsUserAuthor = userId == c.UserId,
                    Replies = c.Replies
                        .OrderBy(r => r.Date)
                        .Take(CommentRepliesPageCount)
                        .Select(r => new ProductCommentReplyViewModel
                        {
                            Id = r.Id,
                            Text = r.Text,
                            UserName = userId != null && userId == r.UserId ? "Вие" : r.User.Name!,
                            Date = r.Date,
                            LastEdited = r.LastEdited,
                            VerifiedPurchase = r.VerifiedPurchase,
                            Likes = r.Reactions.Count(x => x.Reaction == CommentReaction.Like),
                            Dislikes = r.Reactions.Count(x => x.Reaction == CommentReaction.Dislike),
                            UserReaction = userId == null
                                ? null
                                : r.Reactions.FirstOrDefault(x => x.UserId == userId)?.Reaction,
                            IsWriterAdmin = _userManager.IsInRoleAsync(r.User, AdminRoleName).GetAwaiter().GetResult(),
                            IsUserAuthor = userId == r.UserId,
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
                    .ThenInclude(pr => pr!.User)
            .FirstOrDefaultAsync();

        if (comment is null) return ServiceResult<ReplyPageViewModel>.BadRequest();

        ReplyPageViewModel model = new()
        {
            HasMoreReplies = comment.Replies.Count > page * CommentRepliesPageCount,
            Replies = comment.Replies
                .OrderBy(r => r.Date)
                .Skip((page - 1) * CommentRepliesPageCount)
                .Take(CommentRepliesPageCount)
                .Select(r => new ProductCommentReplyViewModel
                {
                    Id = r.Id,
                    Text = r.Text,
                    UserName = userId != null && userId == r.UserId ? "Вие" : r.User.Name!,
                    Date = r.Date,
                    LastEdited = r.LastEdited,
                    VerifiedPurchase = r.VerifiedPurchase,
                    Likes = r.Reactions.Count(x => x.Reaction == CommentReaction.Like),
                    Dislikes = r.Reactions.Count(x => x.Reaction == CommentReaction.Dislike),
                    UserReaction = userId == null
                        ? null
                        : r.Reactions.FirstOrDefault(x => x.UserId == userId)?.Reaction,
                    IsWriterAdmin = _userManager.IsInRoleAsync(r.User, AdminRoleName).GetAwaiter().GetResult(),
                    IsUserAuthor = userId == r.UserId,
                    ParentCommentId = r.ParentCommentId,
                    ProductId = r.ProductId,
                    ReplyCommentWriterName = r.ParentReply is not null
                        ? userId == r.ParentReply!.UserId ? "Вие" : r.ParentReply!.User.Name!
                        : null
                }).ToArray()
        };

        return ServiceResult<ReplyPageViewModel>.Ok(model);
    }

    public async Task<ServiceResult> DeleteCommentAsync(string userId, Guid commentId)
    {
        ProductComment? comment = _repository
            .All<ProductComment>()
            .Include(c => c.Replies)
            .FirstOrDefault(c => c.Id == commentId && c.UserId == userId);

        if (comment is null) return ServiceResult.BadRequest();
        if (comment.UserId != userId) return ServiceResult.Forbidden();

        comment.IsDeleted = true;
        foreach (ProductComment reply in comment.Replies) reply.IsDeleted = true;

        await _repository.SaveChangesAsync();

        await _cache.RemoveAsync(ProductDetailsUserDataKey(comment.ProductId, userId));
        await _cache.RemoveAsync(ProductCommentsKey(comment.ProductId));

        return ServiceResult.Ok();
    }

    public async Task<decimal> GetAverageRatingAsync(Guid productId)
        => await _repository
            .AllReadonly<ProductComment>()
            .Where(c => c.ProductId == productId)
            .AverageAsync(c => (decimal?)c.Rating) ?? 0m;
}