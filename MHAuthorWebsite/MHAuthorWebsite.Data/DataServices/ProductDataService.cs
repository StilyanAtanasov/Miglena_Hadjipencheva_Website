using MHAuthorWebsite.Core.Contracts.DataServices;
using MHAuthorWebsite.Core.Dtos.Product;
using MHAuthorWebsite.Core.Dtos.ProductComment;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using MHAuthorWebsite.Core.Models.Enums;
using Microsoft.EntityFrameworkCore;

using static MHAuthorWebsite.GCommon.ApplicationRules.ProductComment;

namespace MHAuthorWebsite.Data.DataServices;

public class ProductDataService : IProductDataService
{
    private readonly IApplicationRepository _repository;

    public ProductDataService(IApplicationRepository repository) => _repository = repository;

    public async Task<Product?> GetProductWithLikesForEditByIdAsync(Guid productId)
     => await _repository
         .All<Product>()
         .Include(p => p.Likes)
         .FirstOrDefaultAsync(p => p.Id == productId);

    public async Task<ProductDetailsGeneralInfoDto?> GetProductDetailsGeneralInfoByIdAsync(Guid productId, bool includeNonPublicProducts)
    {
        IQueryable<Product> query = _repository.AllReadonly<Product>();

        if (includeNonPublicProducts) query = query.IgnoreQueryFilters();

        return await query
            .Where(p => !p.IsDeleted && p.Id == productId)
            .Select(product => new ProductDetailsGeneralInfoDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Discount = product.Discounts
                    .Where(d => d.StartDate <= DateTime.UtcNow && d.EndDate >= DateTime.UtcNow)
                    .OrderByDescending(d => d.NewPrice)
                    .Select(d => new ProductDetailsDiscountDto
                    {
                        NewPrice = d.NewPrice,
                        EndDate = d.EndDate
                    })
                    .FirstOrDefault(),
                IsInStock = product.StockQuantity > 0,
                IsPublic = product.IsPublic,
                Quantity = product.StockQuantity,
                ProductTypeName = product.ProductType.Name,
                Images = product.Images
                    .Where(i => i.Id != product.Thumbnail.ImageId)
                    .OrderByDescending(i => i.Id == product.Thumbnail.ImageOriginalId)
                    .Select(i => new ProductDetailsImageDto
                    {
                        ImageUrl = i.ImageUrl,
                        AltText = i.AltText
                    })
                    .ToHashSet(),
                Attributes = product.Attributes
                    .Select(a => new ProductAttributeDetailsDto
                    {
                        Label = a.AttributeDefinition.Key,
                        Value = a.Value == null && a.ProductAttributeOptionId != null
                            ? a.ProductAttributeOption!.Value
                            : a.Value,
                        AttributeType = a.AttributeDefinition.DataType,
                        DisplayPosition = a.DisplayPosition
                    })
                    .ToArray()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ProductDetailsCommentsInfoDto?> GetProductDetailsCommentsInfoByIdAsync(Guid productId, bool includeNonPublicProducts, HashSet<string> adminIdSet)
    {
        IQueryable<Product> query = _repository.AllReadonly<Product>();

        if (includeNonPublicProducts) query = query.IgnoreQueryFilters();

        return await query
            .Where(p => p.Id == productId)
            .Select(product => new ProductDetailsCommentsInfoDto
            {
                CommitId = Guid.NewGuid(),
                HasMoreComments = product.Comments.Count(c => c.ParentCommentId == null) > CommentPageCount,
                AverageRating = product.Comments.Any(c => c.ParentCommentId == null && c.Rating.HasValue)
                    ? (decimal)Math.Round(product.Comments
                        .Where(c => c.ParentCommentId == null && c.Rating.HasValue && !c.IsDeleted && !c.User.IsDeleted)
                        .Average(c => c.Rating!.Value), 2)
                    : 0m,
                TotalBaseComments = product.Comments.Count(c => c.ParentCommentId == null && !c.IsDeleted && !c.User.IsDeleted),
                CommentsCountByStarsRating = product.Comments
                    .Where(c => c.Rating.HasValue && c.ParentCommentId == null && !c.IsDeleted && !c.User.IsDeleted)
                    .GroupBy(c => c.Rating!.Value)
                    .Select(g => new StarCountDto
                    {
                        Star = g.Key,
                        Count = g.Count()
                    })
                    .ToArray(),
                Comments = product.Comments
                .Where(c => c.ParentCommentId == null && !c.IsDeleted && !c.User.IsDeleted)
                .OrderByDescending(c => c.Reactions.Count(r => r.Reaction == CommentReaction.Like))
                .ThenByDescending(c => c.Rating)
                .ThenByDescending(c => c.Date)
                .ThenByDescending(c => c.Replies.Count)
                .Take(CommentPageCount)
                .Select(c => new ProductBaseCommentGeneralInfoDto
                {
                    Id = c.Id,
                    Rating = c.Rating!.Value,
                    ProductId = c.ProductId,
                    Text = c.Text,
                    UserName = c.User.Name!,
                    Date = c.Date,
                    LastEdited = c.LastEdited,
                    VerifiedPurchase = c.VerifiedPurchase,
                    Likes = c.Reactions
                        .Count(r => r.Reaction == CommentReaction.Like),
                    Dislikes = c.Reactions
                        .Count(r => r.Reaction == CommentReaction.Dislike),
                    ImageUrls = c.Images
                        .Select(i => i.PreviewUrl)
                        .ToArray(),
                    HasMoreReplies = c.Replies.Count > CommentRepliesPageCount,
                    TotalRepliesCount = c.Replies.Count,
                    Replies = c.Replies
                        .OrderBy(r => r.Date)
                        .Take(CommentRepliesPageCount)
                        .Select(r => new ProductCommentReplyGeneralInfoDto
                        {
                            Id = r.Id,
                            Text = r.Text,
                            UserName = r.User.Name!,
                            Date = r.Date,
                            LastEdited = r.LastEdited,
                            VerifiedPurchase = r.VerifiedPurchase,
                            Likes = r.Reactions.Count(x => x.Reaction == CommentReaction.Like),
                            Dislikes = r.Reactions.Count(x => x.Reaction == CommentReaction.Dislike),
                            ParentCommentId = r.ParentCommentId,
                            ProductId = r.ProductId,
                            ReplyCommentWriterName = r.ParentReply != null ? r.ParentReply!.User.Name! : null,
                            IsWriterAdmin = adminIdSet.Contains(r.UserId),
                        }).ToArray()
                })
                .ToArray()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ProductDetailsUserInfoDto?> GetProductDetailsUserInfoByIdAsync(Guid productId, bool includeNonPublicProducts, string userId, Guid commitId)
    {
        IQueryable<Product> query = _repository.AllReadonly<Product>();

        if (includeNonPublicProducts) query = query.IgnoreQueryFilters();

        return await query
            .Where(p => !p.IsDeleted && p.Id == productId)
            .Select(product => new ProductDetailsUserInfoDto
            {
                CacheGeneralInfoCommitId = commitId,
                IsLiked = product.Likes.Any(u => u.Id == userId),
                CanWriteMoreComments = !product.Comments.Any(c => c.UserId == userId && c.ParentCommentId == null),
                IsRateLimitedForReplies = product.Comments
                    .Count(c => c.UserId == userId && c.ParentCommentId != null
                                                   && c.Date > DateTime.UtcNow
                                                       .AddHours(-MaxRepliesTimeFrameHours)) > MaxRepliesForTimeFrame,
                CommentSpecificInfo = product.Comments
                    .Where(c => c.ParentCommentId == null)
                    .OrderByDescending(c => c.Reactions.Count(r => r.Reaction == CommentReaction.Like))
                    .ThenByDescending(c => c.Rating)
                    .ThenByDescending(c => c.Date)
                    .ThenByDescending(c => c.Replies.Count)
                    .Take(CommentPageCount)
                    .Select(c => new ProductBaseCommentUserInfoDto
                    {
                        Id = c.Id,
                        IsUserAuthor = userId == c.UserId,
                        UserReaction = c.Reactions.Where(r => r.UserId == userId)
                            .Select(r => (CommentReaction?)r.Reaction)
                            .FirstOrDefault(),
                        Replies = c.Replies
                            .OrderBy(r => r.Date)
                            .Take(CommentRepliesPageCount)
                            .Select(r => new ProductCommentReplyUserInfoDto
                            {
                                Id = r.Id,
                                UserReaction = r.Reactions.Where(rx => rx.UserId == userId)
                                    .Select(rx => (CommentReaction?)rx.Reaction)
                                    .FirstOrDefault(),
                                IsUserAuthor = userId == r.UserId
                            })
                            .ToArray()
                    })
                    .ToArray()
            })
            .FirstOrDefaultAsync();
    }
}