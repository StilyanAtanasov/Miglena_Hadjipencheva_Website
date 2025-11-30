using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Data.Models.Enums;
using MHAuthorWebsite.Data.Shared;
using MHAuthorWebsite.Web.ViewModels.Product;
using MHAuthorWebsite.Web.ViewModels.ProductComment;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Linq.Expressions;
using System.Text.Json;
using static MHAuthorWebsite.GCommon.ApplicationRules.CacheKeys;
using static MHAuthorWebsite.GCommon.ApplicationRules.Pagination;
using static MHAuthorWebsite.GCommon.ApplicationRules.ProductComment;

namespace MHAuthorWebsite.Core;

public class ProductService : IProductService
{
    protected readonly IFastCacheService Cache;
    protected readonly IApplicationRepository Repository;
    protected readonly UserManager<ApplicationUser> UserManager;
    protected readonly IGlobalCacheKeysManagementService GlobalCacheKeysManagementService;

    public ProductService(IFastCacheService cacheService, IGlobalCacheKeysManagementService globalCacheKeysManagementService,
        IApplicationRepository repository, UserManager<ApplicationUser> userManager)
    {
        Cache = cacheService;
        GlobalCacheKeysManagementService = globalCacheKeysManagementService;
        Repository = repository;
        UserManager = userManager;
    }

    public async Task<int> GetAllProductsCountAsync() => await Repository.CountAsync<Product>();

    public async Task<ServiceResult<ProductDetailsViewModel>> GetProductDetailsReadonlyAsync(Guid productId, string? userId)
    {
        try
        {
            ProductDetailsGeneralInfoViewModel? generalInfo = await Cache.GetAsync<ProductDetailsGeneralInfoViewModel>(ProductDetailsKey(productId));

            if (generalInfo is null)
            {
                generalInfo = await Repository
                    .AllReadonly<Product>()
                    .Where(p => !p.IsDeleted && p.Id == productId)
                    .Select(product => new ProductDetailsGeneralInfoViewModel
                    {
                        CacheCommitId = Guid.NewGuid(),
                        Id = product.Id,
                        Name = product.Name,
                        Description = product.Description,
                        Price = product.Price,
                        Discount = product.Discounts
                            .Where(d => d.StartDate <= DateTime.Now && d.EndDate >= DateTime.Now)
                            .OrderByDescending(d => d.NewPrice)
                            .Select(d => new ProductDetailsDiscountViewModel
                            {
                                NewPrice = d.NewPrice,
                                EndDate = d.EndDate
                            })
                            .FirstOrDefault(),
                        IsInStock = product.StockQuantity > 0,
                        ProductTypeName = product.ProductType.Name,
                        Images = product.Images
                    .Where(i => i.Id != product.Thumbnail.ImageId)
                    .OrderByDescending(i => i.Id == product.Thumbnail.ImageOriginalId)
                    .Select(i => new ProductDetailsImageViewModel
                    {
                        ImageUrl = i.ImageUrl,
                        AltText = i.AltText
                    })
                    .ToHashSet(),
                        Attributes = product.Attributes
                        .Select(a => new ProductAttributeDetailsViewModel
                        {
                            Label = a.Key,
                            Value = a.Value
                        })
                    .ToArray()
                    })
                    .FirstOrDefaultAsync();

                if (generalInfo is null) return ServiceResult<ProductDetailsViewModel>.NotFound();

                Cache.SetFireAndForget(ProductDetailsKey(productId),
                    generalInfo,
                    TimeSpan.FromDays(3));
            }

            ProductDetailsCommentsInfoViewModel? commentsInfo = await Cache.GetAsync<ProductDetailsCommentsInfoViewModel>(ProductCommentsKey(productId));

            if (commentsInfo is null)
            {
                HashSet<string> adminIdSet =
                    new(await GlobalCacheKeysManagementService.GetAllAdminIdsAsync(), StringComparer.Ordinal);

                commentsInfo = await Repository
                    .AllReadonly<Product>()
                    .Where(p => !p.IsDeleted && p.Id == productId)
                    .Select(product => new ProductDetailsCommentsInfoViewModel
                    {
                        HasMoreComments = product.Comments.Count > CommentPageCount,
                        AverageRating = product.Comments.Any(c => c.ParentCommentId == null && c.Rating.HasValue)
                            ? (decimal)Math.Round(product.Comments
                                .Where(c => c.ParentCommentId == null && c.Rating.HasValue)
                                .Average(c => c.Rating!.Value), 2)
                            : 0m,
                        TotalBaseComments = product.Comments.Count(c => c.ParentCommentId == null),
                        CommentsCountByStarsRating = product.Comments
                            .Where(c => c.Rating.HasValue && c.ParentCommentId == null)
                            .GroupBy(c => c.Rating!.Value)
                            .Select(g => new StarCountViewModel
                            {
                                Star = g.Key,
                                Count = g.Count()
                            })
                            .ToArray(),
                        Comments = product.Comments
                        .Where(c => c.ParentCommentId == null)
                        .OrderByDescending(c => c.Reactions.Count(r => r.Reaction == CommentReaction.Like))
                        .ThenByDescending(c => c.Rating)
                        .ThenByDescending(c => c.Date)
                        .ThenByDescending(c => c.Replies.Count)
                        .Take(CommentPageCount)
                        .Select(c => new ProductBaseCommentGeneralInfoViewModel
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
                                .Select(r => new ProductCommentReplyGeneralInfoViewModel
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

                Cache.SetFireAndForget(ProductCommentsKey(productId),
                    commentsInfo,
                    TimeSpan.FromDays(3));
            }

            ProductDetailsViewModel model = new()
            {
                Id = generalInfo.Id,
                Name = generalInfo.Name,
                Description = generalInfo.Description,
                Price = generalInfo.Price,
                Discount = generalInfo.Discount,
                IsInStock = generalInfo.IsInStock,
                Images = generalInfo.Images,
                ProductTypeName = generalInfo.ProductTypeName,
                HasMoreComments = commentsInfo!.HasMoreComments,
                AverageRating = commentsInfo.AverageRating,
                TotalBaseComments = commentsInfo.TotalBaseComments,
                CommentsCountByStarsRating = Enumerable.Range(1, 5)
                    .ToDictionary(
                        x => x,
                        x => commentsInfo.CommentsCountByStarsRating
                            .FirstOrDefault(c => c.Star == x)?.Count ?? 0),
                Attributes = generalInfo.Attributes,

                // Default values in case the user is not logged in
                CanWriteMoreComments = true,
                IsRateLimitedForReplies = false,
                IsLiked = false,
                Comments = commentsInfo.Comments
                    .Select(c => new ProductBaseCommentViewModel
                    {
                        Id = c.Id,
                        Rating = c.Rating,
                        ProductId = c.ProductId,
                        Text = c.Text,
                        UserName = c.UserName,
                        Date = c.Date,
                        LastEdited = c.LastEdited,
                        VerifiedPurchase = c.VerifiedPurchase,
                        Likes = c.Likes,
                        Dislikes = c.Dislikes,
                        ImageUrls = c.ImageUrls,
                        HasMoreReplies = c.HasMoreReplies,
                        TotalRepliesCount = c.TotalRepliesCount,
                        IsUserAuthor = false,
                        UserReaction = null,
                        Replies = c.Replies
                            .Select(r => new ProductCommentReplyViewModel
                            {
                                Id = r.Id,
                                Text = r.Text,
                                UserName = r.UserName,
                                Date = r.Date,
                                LastEdited = r.LastEdited,
                                VerifiedPurchase = r.VerifiedPurchase,
                                Likes = r.Likes,
                                Dislikes = r.Dislikes,
                                ParentCommentId = r.ParentCommentId,
                                ProductId = r.ProductId,
                                ReplyCommentWriterName = r.ReplyCommentWriterName,
                                IsWriterAdmin = r.IsWriterAdmin,
                                IsUserAuthor = false,
                                UserReaction = null,
                            })
                            .ToArray()
                    })
                    .ToArray()
            };

            if (userId is null) return ServiceResult<ProductDetailsViewModel>.Ok(model);

            ProductDetailsUserInfoViewModel? userInfo =
                await Cache.GetAsync<ProductDetailsUserInfoViewModel>(ProductDetailsUserDataKey(productId, userId));

            if (userInfo is null || generalInfo.CacheCommitId != userInfo.CacheGeneralInfoCommitId)
            {
                userInfo = await Repository
                    .AllReadonly<Product>()
                    .Include(p => p.Likes)
                    .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                    .Include(p => p.Comments)
                    .ThenInclude(c => c.Replies)
                    .ThenInclude(r => r.User)
                    .Include(p => p.Comments)
                    .ThenInclude(c => c.Replies)
                    .ThenInclude(r => r.Reactions)
                    .Include(p => p.Comments)
                    .ThenInclude(c => c.Reactions)
                    .Where(p => !p.IsDeleted && p.Id == productId)
                    .Select(product => new ProductDetailsUserInfoViewModel
                    {
                        CacheGeneralInfoCommitId = generalInfo.CacheCommitId,
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
                        .Select(c => new ProductBaseCommentUserInfoViewModel
                        {
                            Id = c.Id,
                            IsUserAuthor = userId == c.UserId,
                            UserReaction = c.Reactions.Where(r => r.UserId == userId)
                                .Select(r => (CommentReaction?)r.Reaction)
                                .FirstOrDefault(),
                            Replies = c.Replies
                                .OrderBy(r => r.Date)
                                .Take(CommentRepliesPageCount)
                                .Select(r => new ProductCommentReplyUserInfoViewModel
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

                Cache.SetFireAndForget(
                    ProductDetailsUserDataKey(productId, userId),
                    userInfo,
                    TimeSpan.FromDays(2));
            }

            model.CanWriteMoreComments = userInfo!.CanWriteMoreComments;
            model.IsLiked = userInfo.IsLiked;
            model.IsRateLimitedForReplies = userInfo.IsRateLimitedForReplies;

            foreach (ProductBaseCommentViewModel comment in model.Comments)
            {
                ProductBaseCommentUserInfoViewModel matchingUserInfo =
                    userInfo.CommentSpecificInfo.First(c => comment.Id == c.Id);

                comment.UserReaction = matchingUserInfo.UserReaction;
                comment.IsUserAuthor = matchingUserInfo.IsUserAuthor;
                comment.UserName = comment.IsUserAuthor ? "Вие" : comment.UserName;

                foreach (ProductCommentReplyViewModel reply in comment.Replies)
                {
                    ProductCommentReplyUserInfoViewModel matchingReplyUserInfo =
                        matchingUserInfo.Replies.First(r => reply.Id == r.Id);

                    reply.IsUserAuthor = matchingReplyUserInfo.IsUserAuthor;
                    reply.UserReaction = matchingReplyUserInfo.UserReaction;
                    reply.UserName = reply.IsUserAuthor ? "Вие" : reply.UserName;
                }
            }

            return ServiceResult<ProductDetailsViewModel>.Ok(model);
        }
        catch (Exception)
        {
            return ServiceResult<ProductDetailsViewModel>.Failure();
        }
    }

    public async Task<ICollection<LikedProductViewModel>> GetLikedProductsReadonlyAsync(string userId)
    {
        Guid stateId = await GlobalCacheKeysManagementService.DiscountsGlobalStateId();
        LikedProductsListViewModel? likedProducts = await Cache.GetAsync<LikedProductsListViewModel>(LikedProductsKey(userId));
        if (likedProducts is not null && likedProducts.DiscountStateId == stateId) return likedProducts.LikedProducts;

        likedProducts = new()
        {
            DiscountStateId = stateId,
            LikedProducts = await Repository
                .WhereReadonly<Product>(p => p.Likes.Any(u => u.Id == userId))
                .Include(p => p.Thumbnail)
                .ThenInclude(t => t.Image)
                .Include(p => p.ProductType)
                .Include(p => p.Discounts)
                .Select(p => new LikedProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    DiscountedPrice = p.Discounts.Any(d => d.StartDate <= DateTime.Now && d.EndDate >= DateTime.Now)
                        ? p.Discounts.First(d => d.StartDate <= DateTime.Now && d.EndDate >= DateTime.Now).NewPrice
                        : null,
                    CategoryName = p.ProductType.Name,
                    IsInStock = p.StockQuantity > 0,
                    ThumbnailUrl = p.Thumbnail.Image.ImageUrl,
                    ThumbnailAlt = p.Thumbnail.Image.AltText
                })
                .ToArrayAsync()
        };

        Cache.SetFireAndForget(LikedProductsKey(userId), likedProducts, TimeSpan.FromDays(1));
        return likedProducts.LikedProducts;
    }

    public async Task<ICollection<ProductCardViewModel>> GetAllProductCardsReadonlyAsync(string? userId, int page,
        (bool descending, Expression<Func<Product, object>>? expression) sortType)
    {
        Guid[] pagedProductIds = await Repository
            .GetPagedAsync(page, PageSize, true, null, sortType.expression, sortType.descending)
            .Select(p => p.Id)
            .ToArrayAsync();

        if (!pagedProductIds.Any()) return Array.Empty<ProductCardViewModel>();

        ICollection<ProductCardGeneralInfoViewModel> productCards = await GetProductDetailsBatchAsync(pagedProductIds);

        ProductCardViewModel[] productCardViewModels = productCards
            .Select(pc => new ProductCardViewModel
            {
                Id = pc.Id,
                Name = pc.Name,
                Price = pc.Price,
                IsAvailable = pc.IsAvailable,
                ProductType = pc.ProductType,
                ImageUrl = pc.ImageUrl,
                ImageAlt = pc.ImageAlt,
                DiscountPrice = pc.DiscountPrice,
                IsLiked = false // Will be updated later if userId is provided
            })
            .ToArray();

        if (userId != null)
        {
            Guid[] likedProductIds = await Repository
                .WhereReadonly<Product>(p => pagedProductIds.Contains(p.Id) && p.Likes.Any(u => u.Id == userId))
                .Select(p => p.Id)
                .ToArrayAsync();

            foreach (ProductCardViewModel card in productCardViewModels) card.IsLiked = likedProductIds.Contains(card.Id);
        }

        // Ensure the results are returned in the exact order determined by the DB in Step 1.
        return productCardViewModels
            .OrderBy(r => Array.IndexOf(pagedProductIds, r.Id))
            .ToList();
    }

    public async Task<ServiceResult> ToggleLikeProduct(string userId, Guid productId)
    {
        Product? product = await Repository
            .All<Product>()
            .Include(p => p.Likes)
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product is null) return ServiceResult.NotFound();

        ApplicationUser? user = await UserManager.FindByIdAsync(userId);
        if (user is null) return ServiceResult.Forbidden();

        if (product.Likes.All(u => u.Id != userId)) product.Likes.Add(user);
        else product.Likes.Remove(user);

        await Repository.SaveChangesAsync();

        await Cache.RemoveAsync(LikedProductsKey(userId));
        await Cache.RemoveAsync(ProductDetailsUserDataKey(productId, userId));

        return ServiceResult.Ok();
    }

    private async Task<ICollection<ProductCardGeneralInfoViewModel>> GetProductDetailsBatchAsync(Guid[] productIds)
    {
        RedisKey[] keys = productIds
            .Select(id => (RedisKey)ProductCardKey(id))
            .ToArray();

        IBatch batch = Cache.CreateBatch();
        Task<RedisValue>[] tasks = keys.Select(key => batch.StringGetAsync(key)).ToArray();
        batch.Execute();

        RedisValue[] cachedValues = await Task.WhenAll(tasks);
        List<ProductCardGeneralInfoViewModel> cachedProducts = new();
        List<Guid> missingIds = new();

        for (int i = 0; i < productIds.Length; i++)
        {
            if (cachedValues[i].HasValue)
            {
                ProductCardGeneralInfoViewModel? product = JsonSerializer.Deserialize<ProductCardGeneralInfoViewModel>(cachedValues[i].ToString());
                if (product != null) cachedProducts.Add(product);
            }
            else missingIds.Add(productIds[i]);
        }

        if (missingIds.Any())
        {
            ProductCardGeneralInfoViewModel[] dbItems = await Repository.WhereReadonly<Product>(p => missingIds.Contains(p.Id))
                 .Include(p => p.ProductType)
                 .Include(p => p.Thumbnail).ThenInclude(t => t.Image)
                 .Include(p => p.Discounts)
                 .Select(p => new ProductCardGeneralInfoViewModel
                 {
                     Id = p.Id,
                     Name = p.Name,
                     Price = p.Price,
                     IsAvailable = p.StockQuantity > 0,
                     ProductType = p.ProductType.Name,
                     ImageUrl = p.Thumbnail.Image.ImageUrl,
                     ImageAlt = p.Thumbnail.Image.AltText,
                     DiscountPrice = p.Discounts.Any(d => d.StartDate <= DateTime.Now && d.EndDate >= DateTime.Now)
                         ? p.Discounts.First(d => d.StartDate <= DateTime.Now && d.EndDate >= DateTime.Now).NewPrice
                         : null
                 })
                 .ToArrayAsync();

            IBatch writeBatch = Cache.CreateBatch();
            foreach (ProductCardGeneralInfoViewModel item in dbItems)
            {
                string json = JsonSerializer.Serialize(item);
                await writeBatch.StringSetAsync((RedisKey)ProductCardKey(item.Id), (RedisValue)json, TimeSpan.FromDays(2), flags: CommandFlags.FireAndForget);
                cachedProducts.Add(item);
            }

            writeBatch.Execute();
        }

        return cachedProducts;
    }
}