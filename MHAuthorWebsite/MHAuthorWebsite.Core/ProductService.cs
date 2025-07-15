using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dto;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Data.Shared;
using MHAuthorWebsite.Web.ViewModels.Product;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

    public async Task<ServiceResult> AddProductAsync(AddProductForm model)
    {
        try
        {
            Product product = new()
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                ProductTypeId = model.ProductTypeId,
                StockQuantity = model.StockQuantity,
            };

            await _repository.AddAsync(product);
            await _repository.SaveChangesAsync();

            if (model.Attributes.Any())
            {
                ICollection<ProductAttribute> attributes = model.Attributes
                    .Select(a => new ProductAttribute
                    {
                        Key = a.Key,
                        Value = a.Value,
                        ProductId = product.Id
                    })
                    .ToArray();

                await _repository.AddRangeAsync(attributes);
                await _repository.SaveChangesAsync();
            }
            return ServiceResult.Ok();
        }
        catch (Exception)
        {
            return ServiceResult.Failure();
        }
    }

    public async Task<ServiceResult<ProductDetailsViewModel>> GetProductDetailsReadonlyAsync(Guid productId)
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

    public async Task<ICollection<ProductCardViewModel>> GetAllProductCardsReadonlyAsync(string? userId) =>
        await _repository
            .AllReadonly<Product>()
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

    public async Task<ICollection<ProductTypeAttributesDto>> GetProductTypeAttributesAsync(int productTypeId) =>
        await _repository
            .Where<ProductAttributeDefinition>(pad => pad.ProductTypeId == productTypeId)
            .Select(pad => new ProductTypeAttributesDto
            {
                Key = pad.Key,
                Label = pad.Label,
                DataType = (int)pad.DataType,
                HasPredefinedValue = pad.HasPredefinedValue,
                IsRequired = pad.IsRequired
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

    public async Task<ServiceResult> DeleteProductAsync(Guid productId)
    {
        try
        {
            Product? product = await _repository.GetByIdAsync<Product>(productId);
            if (product is null) return ServiceResult.NotFound();

            product.IsDeleted = true;
            await _repository.SaveChangesAsync();
            return ServiceResult.Ok();
        }
        catch (Exception)
        {
            return ServiceResult.Failure();
        }
    }
}