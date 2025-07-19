using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dto;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Data.Shared;
using MHAuthorWebsite.Web.ViewModels.Product;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
                Attributes = product.Attributes
                    .Select(a => new ProductAttributeDetailsViewModel
                    {
                        Label = a.Key,
                        Value = a.Value ?? _repository.FindByExpressionAsync<ProductAttributeOption>(pao => pao.Id == a.ProductAttributeOptionsId).Result!.Value // TODO : experiment when ProductAttributeOptionsId is used
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
            .Select(p => new LikedProductViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                ShortDescription = p.Description.Length > 100 ? $"{p.Description.Substring(0, 100)}..." : p.Description,
                IsInStock = p.StockQuantity > 0,
            })
            .ToArrayAsync();

    public async Task<ICollection<ProductCardViewModel>> GetAllProductCardsReadonlyAsync(string? userId, int page) =>
        await _repository
            .GetPagedAsync<Product>(page, PageSize)
            .Include(p => p.ProductType)
            .Select(p => new ProductCardViewModel
            {
                Id = p.Id,
                Name = p.Name,
                ShortDescription = p.Description.Length > 100 ? $"{p.Description.Substring(0, 100)}..." : p.Description,
                Price = p.Price,
                IsAvailable = p.StockQuantity > 0,
                ProductType = p.ProductType.Name,
                IsLiked = userId != null && p.Likes.Any(u => u.Id == userId)
            })
            .ToArrayAsync();

    public async Task<ICollection<ProductListViewModel>> GetProductsListReadonlyAsync() =>
        await _repository
            .AllReadonly<Product>()
            .IgnoreQueryFilters()
            .Include(p => p.ProductType)
            .Where(p => !p.IsDeleted)
            .Select(p => new ProductListViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                ProductTypeName = p.ProductType.Name,
                IsPublic = p.IsPublic
            })
            .ToArrayAsync();

    public async Task<ServiceResult> ToggleProductPublicityAsync(Guid productId)
    {
        try
        {
            Product? product = await _repository
                .All<Product>()
                .IgnoreQueryFilters()
                .Where(p => !p.IsDeleted)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product is null) return ServiceResult.NotFound();

            product.IsPublic = !product.IsPublic;
            await _repository.SaveChangesAsync();
            return ServiceResult.Ok();
        }
        catch (Exception)
        {
            return ServiceResult.Failure();
        }
    }

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