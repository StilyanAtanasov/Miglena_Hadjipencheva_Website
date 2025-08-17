using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Data.Shared;
using MHAuthorWebsite.Web.ViewModels.Product;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using static MHAuthorWebsite.GCommon.ApplicationRules.Pagination;

namespace MHAuthorWebsite.Core;

public class ProductService : IProductService
{
    private readonly IApplicationRepository _repository;
    private readonly UserManager<IdentityUser> _userManager;

    public ProductService(IApplicationRepository repository, UserManager<IdentityUser> userManager)
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
                    .OrderByDescending(i => i.IsThumbnail)
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
            .Include(p => p.Images)
            .Include(p => p.ProductType)
            .Select(p => new LikedProductViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CategoryName = p.ProductType.Name,
                IsInStock = p.StockQuantity > 0,
                ThumbnailUrl = p.Images.First(i => i.IsThumbnail).ThumbnailUrl!,
                ThumbnailAlt = p.Images.First(i => i.IsThumbnail).AltText,
            })
            .ToArrayAsync();

    public async Task<ICollection<ProductCardViewModel>> GetAllProductCardsReadonlyAsync(string? userId, int page,
        (bool descending, Expression<Func<Product, object>>? expression) sortType) =>
        await _repository
            .GetPagedAsync(page, PageSize, true, null, sortType.expression, sortType.descending)
            .Include(p => p.ProductType)
            .Select(p => new ProductCardViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                IsAvailable = p.StockQuantity > 0,
                ProductType = p.ProductType.Name,
                IsLiked = userId != null && p.Likes.Any(u => u.Id == userId),
                ImageUrl = p.Images.First(i => i.IsThumbnail).ThumbnailUrl!,
                ImageAlt = p.Images.First(i => i.IsThumbnail).AltText
            })
            .ToArrayAsync();

    public async Task<ServiceResult> ToggleLikeProduct(string userId, Guid productId)
    {
        Product? product = await _repository
            .All<Product>()
            .Include(p => p.Likes)
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product is null) return ServiceResult.NotFound();

        IdentityUser? user = await _userManager.FindByIdAsync(userId);
        if (user is null) return ServiceResult.Forbidden();

        if (product.Likes.All(u => u.Id != userId)) product.Likes.Add(user);
        else product.Likes.Remove(user);

        await _repository.SaveChangesAsync();
        return ServiceResult.Ok();
    }
}