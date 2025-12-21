using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Contracts.DataServices;
using MHAuthorWebsite.Core.Dtos.ProductComment;
using MHAuthorWebsite.Core.Extensions;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using MHAuthorWebsite.Core.Models.Enums;
using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;
using static MHAuthorWebsite.GCommon.ApplicationRules.CacheKeys;
using static MHAuthorWebsite.GCommon.ApplicationRules.ProductComment;
using static MHAuthorWebsite.GCommon.ApplicationRules.Roles;

namespace MHAuthorWebsite.Core;

public class ProductCommentService : IProductCommentService
{
    private readonly IFastCacheService _cache;
    private readonly IApplicationRepository _repository;
    private readonly IProductCommentDataService _productCommentDataService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProductCommentService(IFastCacheService cache, IApplicationRepository repository, IProductCommentDataService productCommentDataService, UserManager<ApplicationUser> userManager)
    {
        _cache = cache;
        _repository = repository;
        _productCommentDataService = productCommentDataService;
        _userManager = userManager;
    }

    public async Task<ServiceResult<ProductCommentDetailsDto>> GetCommentDetailsReadonlyAsync(Guid commentId, string? userId)
    {
        ProductCommentDetailsDto? comment = await _repository
            .AllReadonly<ProductComment>()
            .Where(c => c.Id == commentId)
            .Select(c => new ProductCommentDetailsDto
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
                    .Select(i => new ProductCommentImageDto
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
            ? ServiceResult<ProductCommentDetailsDto>.NotFound()
            : ServiceResult<ProductCommentDetailsDto>.Ok(comment);
    }

    public async Task<ServiceResult> AddCommentAsync(string userId, AddProductCommentDto model, ICollection<ProductCommentImagesUploadDto>? images)
    {
        Product? product = await _productCommentDataService.GetProductWithOrdersAndCommentsAsync(model.ProductId);
        if (product is null) return ServiceResult.BadRequest();

        ApplicationUser? user = await _userManager.FindByIdAsync(userId);
        if (user is null || (await _userManager.IsInRoleAsync(user, AdminRoleName) && model.ParentCommentId is null)) return ServiceResult.Forbidden();

        ProductComment? parentComment = model.ParentCommentId != null
            ? await _repository.Where<ProductComment>(c => c.Id == model.ParentCommentId).FirstOrDefaultAsync()
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

    public async Task<ServiceResult<EditProductCommentDto>> GetCommentForEditReadonlyAsync(string userId, Guid commentId)
    {
        ProductComment? comment = await _productCommentDataService.GetCommentForEditReadonlyAsync(commentId, userId);

        if (comment is null) return ServiceResult<EditProductCommentDto>.NotFound();
        if (comment.UserId != userId) return ServiceResult<EditProductCommentDto>.BadRequest();

        EditProductCommentDto model = new()
        {
            CommentId = comment.Id,
            ProductId = comment.ProductId,
            ParentCommentId = comment.ParentCommentId,
            ReplyCommentId = comment.ParentReplyId,
            Rating = comment.Rating,
            Text = comment.Text,
            ImagePreviewUrls = comment.Images.Select(i => new EditProductCommentImageDto
            {
                ImageId = i.Id,
                PreviewUrl = i.PreviewUrl,
            }).ToArray()
        };

        return ServiceResult<EditProductCommentDto>.Ok(model);
    }

    public async Task<ServiceResult<ICollection<string>>> EditCommentAsync(string userId, EditProductCommentDto model, ICollection<ProductCommentImagesUploadDto>? newImages, ICollection<Guid>? removedImagesUrls)
    {
        ProductComment? comment = await _productCommentDataService.GetCommentForEditReadonlyAsync(model.CommentId, userId);

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

    public async Task<ServiceResult<ICollection<ProductCommentReactionDto>>> ReactToComment(string userId, Guid commentId, CommentReaction reactionType)
    {
        ProductComment? comment = await _productCommentDataService.GetCommentForReactionAsync(commentId);
        if (comment is null) return ServiceResult<ICollection<ProductCommentReactionDto>>.BadRequest();

        bool isValidReaction = Enum.IsDefined(typeof(CommentReaction), reactionType);
        if (!isValidReaction) return ServiceResult<ICollection<ProductCommentReactionDto>>.BadRequest();

        if (comment.UserId == userId) return ServiceResult<ICollection<ProductCommentReactionDto>>.Forbidden();

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

        ICollection<ProductCommentReactionDto> reactions = allReactions
            .Select(r => new ProductCommentReactionDto
            {
                Reaction = (int)r,
                Count = comment.Reactions.Count(x => x.Reaction == r)
            })
            .ToArray();

        return ServiceResult<ICollection<ProductCommentReactionDto>>.Ok(reactions);
    }

    public async Task<ServiceResult<CommentPageDto>> LoadCommentsReadonlyAsync(Guid productId, int page, int? ratingFilter, string? userId)
    {
        Product? product = await _productCommentDataService.GetProductForCommentsLoadAsync(productId);

        if (product is null) return ServiceResult<CommentPageDto>.BadRequest();
        CommentPageDto model = new()
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
                .Select(c => new ProductBaseCommentDto
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
                        .Select(r => new ProductCommentReplyDto
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

        return ServiceResult<CommentPageDto>.Ok(model);
    }

    public async Task<ServiceResult<ReplyPageDto>> LoadRepliesReadonlyAsync(Guid productId, Guid commentId, int page, string? userId)
    {
        ProductComment? comment =
            await _productCommentDataService.GetCommentForRepliesLoadReadonlyAsync(productId, commentId);

        if (comment is null) return ServiceResult<ReplyPageDto>.BadRequest();

        ReplyPageDto model = new()
        {
            HasMoreReplies = comment.Replies.Count > page * CommentRepliesPageCount,
            Replies = comment.Replies
                .OrderBy(r => r.Date)
                .Skip((page - 1) * CommentRepliesPageCount)
                .Take(CommentRepliesPageCount)
                .Select(r => new ProductCommentReplyDto
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

        return ServiceResult<ReplyPageDto>.Ok(model);
    }

    public async Task<ServiceResult> DeleteCommentAsync(string userId, Guid commentId)
    {
        ProductComment? comment = await _productCommentDataService
            .GetCommentForDeletionAsync(commentId, userId);

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
        => await _productCommentDataService.GetAverageRatingAsync(productId);
}