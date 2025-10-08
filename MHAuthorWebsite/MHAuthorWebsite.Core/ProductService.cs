using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Data.Models.Enums;
using MHAuthorWebsite.Data.Shared;
using MHAuthorWebsite.Web.ViewModels.Product;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using static MHAuthorWebsite.GCommon.ApplicationRules.Pagination;
using static MHAuthorWebsite.GCommon.ApplicationRules.Roles;

namespace MHAuthorWebsite.Core;

public class ProductService : IProductService
{
    private readonly IApplicationRepository _repository;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProductService(IApplicationRepository repository, UserManager<ApplicationUser> userManager)
    {
        _repository = repository;
        _userManager = userManager;
    }

    public async Task<int> GetAllProductsCountAsync() => await _repository.CountAsync<Product>();

    public async Task<ServiceResult<ProductDetailsViewModel>> GetProductDetailsReadonlyAsync(Guid productId, string? userId)
    {
        try
        {
            Product? product = await _repository
                .AllReadonly<Product>()
                .Include(p => p.ProductType)
                .Include(p => p.Attributes)
                .Include(p => p.Images)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.Reactions)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.Images)
                .FirstOrDefaultAsync(p => !p.IsDeleted && p.Id == productId);

            if (product is null) return ServiceResult<ProductDetailsViewModel>.NotFound();

            ProductDetailsViewModel viewModel = new()
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                IsInStock = product.StockQuantity > 0,
                IsLiked = userId != null && product.Likes.Any(u => u.Id == userId),
                ProductTypeName = product.ProductType.Name,
                Images = product.Images
                    .OrderByDescending(i => i.Id == product.ThumbnailImageId)
                    .Select(i => new ProductDetailsImage
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
                    .ToArray(),
                Comments = product.Comments
                    .Select(c => new ProductCommentViewModel
                    {
                        Id = c.Id,
                        ParentCommentId = c.ParentCommentId,
                        Rating = c.Rating,
                        Text = c.Text,
                        UserName = c.User.Name!,
                        Date = c.Date,
                        VerifiedPurchase = c.VerifiedPurchase,
                        Likes = c.Reactions.Count(r => r.Reaction == CommentReaction.Like),
                        Dislikes = c.Reactions.Count(r => r.Reaction == CommentReaction.Dislike),
                        UserReaction = userId == null ? null : c.Reactions.FirstOrDefault(r => r.UserId == userId)?.Reaction,
                        IsWriterAdmin = _userManager.IsInRoleAsync(c.User, AdminRoleName).GetAwaiter().GetResult(),
                        ImageUrls = c.Images.Select(i => i.ImageUrl).ToArray()
                    })
                    .ToArray()
            };

            return ServiceResult<ProductDetailsViewModel>.Ok(viewModel);
        }
        catch (Exception)
        {
            return ServiceResult<ProductDetailsViewModel>.Failure();
        }
    }

    public async Task<ICollection<LikedProductViewModel>> GetLikedProductsReadonlyAsync(string userId) =>
        await _repository
            .WhereReadonly<Product>(p => p.Likes.Any(u => u.Id == userId))
            .Include(p => p.ThumbnailImage)
            .Include(p => p.ProductType)
            .Select(p => new LikedProductViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CategoryName = p.ProductType.Name,
                IsInStock = p.StockQuantity > 0,
                ThumbnailUrl = p.ThumbnailImage.ImageUrl,
                ThumbnailAlt = p.ThumbnailImage.AltText,
            })
            .ToArrayAsync();

    public async Task<ICollection<ProductCardViewModel>> GetAllProductCardsReadonlyAsync(string? userId, int page,
        (bool descending, Expression<Func<Product, object>>? expression) sortType) =>
        await _repository
            .GetPagedAsync(page, PageSize, true, null, sortType.expression, sortType.descending)
            .Include(p => p.ProductType)
            .Include(p => p.ThumbnailImage)
            .Select(p => new ProductCardViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                IsAvailable = p.StockQuantity > 0,
                ProductType = p.ProductType.Name,
                IsLiked = userId != null && p.Likes.Any(u => u.Id == userId),
                ImageUrl = p.ThumbnailImage.ImageUrl,
                ImageAlt = p.ThumbnailImage.AltText
            })
            .ToArrayAsync();

    public async Task<ServiceResult> ToggleLikeProduct(string userId, Guid productId)
    {
        Product? product = await _repository
            .All<Product>()
            .Include(p => p.Likes)
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product is null) return ServiceResult.NotFound();

        ApplicationUser? user = await _userManager.FindByIdAsync(userId);
        if (user is null) return ServiceResult.Forbidden();

        if (product.Likes.All(u => u.Id != userId)) product.Likes.Add(user);
        else product.Likes.Remove(user);

        await _repository.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> AddCommentAsync(string userId, AddProductCommentViewModel model, ICollection<ImageUploadResultDto> images)
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
            Text = model.Text,
            VerifiedPurchase = product.Orders.Any(o => o.Order.UserId == userId), // TODO confirm order is received
            Date = DateTime.UtcNow,
            Images = images.Select(i => new ProductCommentImage
            {
                ImageUrl = i.ImageUrl,
                PublicId = i.PublicId,
                AltText = model.TargetName
            }).ToList()
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