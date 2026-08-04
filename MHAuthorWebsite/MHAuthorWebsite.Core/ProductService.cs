using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Contracts.DataServices;
using MHAuthorWebsite.Core.Dtos.Product;
using MHAuthorWebsite.Core.Dtos.ProductComment;
using MHAuthorWebsite.Core.Extensions;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using static MHAuthorWebsite.GCommon.ApplicationRules.CacheDefaultDurations;
using static MHAuthorWebsite.GCommon.ApplicationRules.CacheKeys;
using static MHAuthorWebsite.GCommon.ApplicationRules.Pagination;
using static MHAuthorWebsite.GCommon.ApplicationRules.ProductComment;
using static MHAuthorWebsite.GCommon.ApplicationRules.Roles;

namespace MHAuthorWebsite.Core;

public class ProductService : IProductService
{
    protected readonly IFastCacheService Cache;
    protected readonly IApplicationRepository Repository;
    protected readonly IProductDataService ProductDataService;
    protected readonly UserManager<ApplicationUser> UserManager;
    protected readonly IGlobalCacheKeysManagementService GlobalCacheKeysManagementService;

    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IFastCacheService cacheService,
        IProductDataService productDataService,
        IGlobalCacheKeysManagementService globalCacheKeysManagementService,
        IApplicationRepository repository,
        UserManager<ApplicationUser> userManager,
        ILogger<ProductService> logger)
    {
        Cache = cacheService;
        GlobalCacheKeysManagementService = globalCacheKeysManagementService;
        Repository = repository;
        UserManager = userManager;
        ProductDataService = productDataService;
        _logger = logger;
    }

    public async Task<int> GetAllProductsCountAsync(string? searchString)
    {
        IQueryable<Product> query = Repository.AllReadonly<Product>();

        if (!string.IsNullOrWhiteSpace(searchString))
            query = query.Where(p => p.Name.Contains(searchString) || p.Description.Contains(searchString)
            || p.ProductType.Name.Contains(searchString));

        int count = await query.CountAsync();
        _logger.LogInformation("Found {Count} products matching search string: '{SearchString}'.", count, searchString);
        return count;
    }

    public async Task<ServiceResult<ProductDetailsDto>> GetProductDetailsReadonlyAsync(Guid productId, string? userId)
    {
        try
        {
            bool isUserAdmin = userId != null && await UserManager.IsInRoleAsync((await UserManager.FindByIdAsync(userId))!, AdminRoleName);

            ProductDetailsGeneralInfoDto? generalInfo = await Cache.GetAsync<ProductDetailsGeneralInfoDto>(ProductDetailsKey(productId));

            if (generalInfo is not null && !generalInfo.IsPublic && !isUserAdmin)
                return ServiceResult<ProductDetailsDto>.NotFound();

            if (generalInfo is null)
            {
                generalInfo = await ProductDataService.GetProductDetailsGeneralInfoByIdAsync(productId, isUserAdmin);

                if (generalInfo is null) return ServiceResult<ProductDetailsDto>.NotFound();

                TimeSpan productDetailsTtl = TimeSpan.FromDays(ProductDetailsTtlDays);
                if (generalInfo.Discount is not null)
                {
                    TimeSpan timeUntilDiscountEnds = generalInfo.Discount.EndDate - DateTime.UtcNow;
                    if (timeUntilDiscountEnds > productDetailsTtl) productDetailsTtl = timeUntilDiscountEnds;
                }

                Cache.SetFireAndForget(ProductDetailsKey(productId),
                    generalInfo,
                    productDetailsTtl);
            }

            ProductDetailsCommentsInfoDto? commentsInfo = await Cache.GetAsync<ProductDetailsCommentsInfoDto>(ProductCommentsKey(productId));

            if (commentsInfo is null)
            {
                HashSet<string> adminIdSet =
                    new(await GlobalCacheKeysManagementService.GetAllAdminIdsAsync(), StringComparer.Ordinal);

                commentsInfo = await ProductDataService.GetProductDetailsCommentsInfoByIdAsync(productId, isUserAdmin, adminIdSet);

                Cache.SetFireAndForget(ProductCommentsKey(productId),
                    commentsInfo,
                    TimeSpan.FromDays(3));
            }

            ProductDetailsDto model = new()
            {
                Id = generalInfo.Id,
                Name = generalInfo.Name,
                Description = generalInfo.Description,
                Price = generalInfo.Price,
                Discount = generalInfo.Discount,
                IsInStock = generalInfo.IsInStock,
                IsPublic = generalInfo.IsPublic,
                Quantity = generalInfo.Quantity,
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
                    .Select(c => new ProductBaseCommentDto
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
                            .Select(r => new ProductCommentReplyDto
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

            if (userId is null) return ServiceResult<ProductDetailsDto>.Ok(model);

            ProductDetailsUserInfoDto? userInfo =
                await Cache.GetAsync<ProductDetailsUserInfoDto>(ProductDetailsUserDataKey(productId, userId));

            if (userInfo is null || commentsInfo.CommitId != userInfo.CacheGeneralInfoCommitId)
            {
                userInfo = await ProductDataService.GetProductDetailsUserInfoByIdAsync(productId, isUserAdmin, userId, commentsInfo.CommitId);

                Cache.SetFireAndForget(
                    ProductDetailsUserDataKey(productId, userId),
                    userInfo,
                    TimeSpan.FromDays(2));
            }

            model.CanWriteMoreComments = userInfo!.CanWriteMoreComments;
            model.IsLiked = userInfo.IsLiked;
            model.IsRateLimitedForReplies = userInfo.IsRateLimitedForReplies;

            foreach (ProductBaseCommentDto comment in model.Comments)
            {
                ProductBaseCommentUserInfoDto matchingUserInfo =
                    userInfo.CommentSpecificInfo.First(c => comment.Id == c.Id);

                comment.UserReaction = matchingUserInfo.UserReaction;
                comment.IsUserAuthor = matchingUserInfo.IsUserAuthor;
                comment.UserName = comment.IsUserAuthor ? "Вие" : comment.UserName;

                foreach (ProductCommentReplyDto reply in comment.Replies)
                {
                    ProductCommentReplyUserInfoDto matchingReplyUserInfo =
                        matchingUserInfo.Replies.First(r => reply.Id == r.Id);

                    reply.IsUserAuthor = matchingReplyUserInfo.IsUserAuthor;
                    reply.UserReaction = matchingReplyUserInfo.UserReaction;
                    reply.UserName = reply.IsUserAuthor ? "Вие" : reply.UserName;
                }
            }

            _logger.LogInformation("Successfully retrieved product details for ProductId: {ProductId}, UserId: {UserId}", productId, userId);
            return ServiceResult<ProductDetailsDto>.Ok(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve product details for ProductId: {ProductId}", productId);
            return ServiceResult<ProductDetailsDto>.Failure();
        }
    }

    public async Task<ICollection<LikedProductDto>> GetLikedProductsReadonlyAsync(string userId)
    {
        Guid stateId = await GlobalCacheKeysManagementService.DiscountsGlobalStateId();
        LikedProductsListDto? likedProducts = await Cache.GetAsync<LikedProductsListDto>(LikedProductsKey(userId));
        if (likedProducts is not null && likedProducts.DiscountStateId == stateId) return likedProducts.LikedProducts;

        DateTime now = DateTime.UtcNow;
        LikedProductAllServiceDataDto[] likedProductsData = await Repository
            .WhereReadonly<Product>(p => p.Likes.Any(u => u.Id == userId))
            .Select(p => new LikedProductAllServiceDataDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                DiscountedPrice = p.Discounts.Any(d => d.StartDate <= now && d.EndDate >= now)
                    ? p.Discounts.First(d => d.StartDate <= now && d.EndDate >= now).NewPrice
                    : null,
                DiscountEnd = p.Discounts.Any(d => d.StartDate <= now && d.EndDate >= now)
                    ? p.Discounts.First(d => d.StartDate <= now && d.EndDate >= now).EndDate
                    : null,
                CategoryName = p.ProductType.Name,
                IsInStock = p.StockQuantity > 0,
                ThumbnailUrl = p.Thumbnail.Image.ImageUrl,
                ThumbnailAlt = p.Thumbnail.Image.AltText
            })
            .ToArrayAsync();

        likedProducts = new()
        {
            DiscountStateId = stateId,
            LikedProducts = likedProductsData
                .Select(p => new LikedProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    DiscountedPrice = p.DiscountedPrice,
                    CategoryName = p.CategoryName,
                    IsInStock = p.IsInStock,
                    ThumbnailUrl = p.ThumbnailUrl,
                    ThumbnailAlt = p.ThumbnailAlt
                })
                .ToArray()
        };

        DateTime? nearestDiscountEnd = likedProductsData
            .Where(p => p.DiscountEnd > now)
            .Min(p => p.DiscountEnd);

        TimeSpan cacheDuration = nearestDiscountEnd != default && nearestDiscountEnd - now < TimeSpan.FromDays(LikedProductTtlDays)
            ? nearestDiscountEnd.Value - now
            : TimeSpan.FromDays(LikedProductTtlDays);

        Cache.SetFireAndForget(LikedProductsKey(userId), likedProducts, cacheDuration);
        _logger.LogInformation("Successfully retrieved liked products for UserId: {UserId}. Count: {Count}", userId, likedProducts.LikedProducts.Count);
        return likedProducts.LikedProducts;
    }

    public async Task<ICollection<ProductCardDto>> GetAllProductCardsReadonlyAsync(string? userId, int page,
        (bool descending, Expression<Func<Product, object>>? expression) sortType, string? searchString)
    {
        Expression<Func<Product, bool>>? filter = null;

        if (!string.IsNullOrWhiteSpace(searchString))
            filter = p =>
                p.Name.Contains(searchString) || p.Description.Contains(searchString) || p.ProductType.Name.Contains(searchString);

        Guid[] pagedProductIds = await Repository
            .GetPagedAsync(page, StorePageSize, true, filter, sortType.expression, sortType.descending)
            .Select(p => p.Id)
            .ToArrayAsync();

        if (!pagedProductIds.Any()) return Array.Empty<ProductCardDto>();

        ICollection<ProductCardGeneralInfoDto> productCards = await GetProductDetailsBatchAsync(pagedProductIds);

        ProductCardDto[] productCardViewModels = productCards
            .Select(pc => new ProductCardDto
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

            foreach (ProductCardDto card in productCardViewModels) card.IsLiked = likedProductIds.Contains(card.Id);
        }

        // Ensure the results are returned in the exact order determined by the DB in Step 1.
        List<ProductCardDto> result = productCardViewModels
            .OrderBy(r => Array.IndexOf(pagedProductIds, r.Id))
            .ToList();

        _logger.LogInformation("Successfully retrieved {Count} product cards. Page: {Page}, Search: '{SearchString}'", result.Count, page, searchString);

        return result;
    }

    public async Task<ServiceResult> ToggleLikeProduct(string userId, Guid productId)
    {
        Product? product = await ProductDataService.GetProductWithLikesForEditByIdAsync(productId);

        if (product is null) return ServiceResult.NotFound();

        ApplicationUser? user = await UserManager.FindByIdAsync(userId);
        if (user is null) return ServiceResult.Forbidden();

        bool isLiked = product.Likes.Any(u => u.Id == userId);
        if (!isLiked) product.Likes.Add(user);
        else product.Likes.Remove(user);

        await Repository.SaveChangesAsync();

        await Cache.RemoveAsync(LikedProductsKey(userId));
        await Cache.RemoveAsync(ProductDetailsUserDataKey(productId, userId));

        _logger.LogInformation($"User with ID {userId} {(!isLiked ? "liked" : "unliked")} product with ID {productId}.");

        return ServiceResult.Ok();
    }

    private async Task<ICollection<ProductCardGeneralInfoDto>> GetProductDetailsBatchAsync(Guid[] productIds)
    {
        ICollection<string> keys = productIds
            .Select(ProductCardKey)
            .ToList();

        IEnumerable<ProductCardGeneralInfoDto?> cachedValues = await Cache.GetBatchAsync<ProductCardGeneralInfoDto>(keys);

        List<ProductCardGeneralInfoDto> cachedProducts = new();
        List<Guid> missingIds = new();

        var cachedValuesList = cachedValues.ToList();
        for (int i = 0; i < productIds.Length; i++)
        {
            ProductCardGeneralInfoDto? product = cachedValuesList[i];
            if (product != null) cachedProducts.Add(product);
            else missingIds.Add(productIds[i]);
        }

        DateTime now = DateTime.UtcNow;
        if (missingIds.Any())
        {
            ProductCardServiceDataDto[] dbItems = await Repository
                .WhereReadonly<Product>(p => missingIds.Contains(p.Id))
                .Select(p => new ProductCardServiceDataDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    IsAvailable = p.StockQuantity > 0,
                    ProductType = p.ProductType.Name,
                    ImageUrl = p.Thumbnail.Image.ImageUrl,
                    ImageAlt = p.Thumbnail.Image.AltText,
                    DiscountPrice = p.Discounts.Any(d => d.StartDate <= now && d.EndDate >= now)
                         ? p.Discounts.First(d => d.StartDate <= now && d.EndDate >= now).NewPrice
                         : null,
                    DiscountEnd = p.Discounts.Any(d => d.StartDate <= now && d.EndDate >= now)
                         ? p.Discounts.First(d => d.StartDate <= now && d.EndDate >= now).EndDate
                         : null,
                })
                 .ToArrayAsync();

            foreach (ProductCardServiceDataDto item in dbItems)
            {
                ProductCardGeneralInfoDto dto = new()
                {
                    Id = item.Id,
                    Name = item.Name,
                    Price = item.Price,
                    IsAvailable = item.IsAvailable,
                    ProductType = item.ProductType,
                    ImageUrl = item.ImageUrl,
                    ImageAlt = item.ImageAlt,
                    DiscountPrice = item.DiscountPrice
                };

                TimeSpan cacheDuration = item.DiscountEnd != null && item.DiscountEnd.Value - now < TimeSpan.FromDays(ProductCardTtlDays)
                    ? item.DiscountEnd.Value - now
                    : TimeSpan.FromDays(ProductCardTtlDays);

                await Cache.SetAsync(ProductCardKey(dto.Id), dto, cacheDuration);
                cachedProducts.Add(dto);
            }
        }

        return cachedProducts;
    }
}