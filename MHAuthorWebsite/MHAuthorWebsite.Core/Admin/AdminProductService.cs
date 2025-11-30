using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Data.Shared;
using MHAuthorWebsite.Web.ViewModels.Admin.Product;
using MHAuthorWebsite.Web.ViewModels.Product;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static MHAuthorWebsite.GCommon.ApplicationRules.CacheKeys;

namespace MHAuthorWebsite.Core.Admin;

public class AdminProductService : ProductService, IAdminProductService
{
    public AdminProductService(IFastCacheService cacheService, IApplicationRepository repository,
        IGlobalCacheKeysManagementService globalCacheKeysManagementService,
        UserManager<ApplicationUser> userManager)
        : base(cacheService, globalCacheKeysManagementService, repository, userManager) { }

    public async Task<ServiceResult> AddProductAsync(AddProductDto model)
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
                Weight = model.Weight
            };

            await Repository.AddAsync(product);
            await Repository.SaveChangesAsync();

            ProductImage[] images = model.ImageUrls.Select(i => new ProductImage
            {
                ProductId = product.Id,
                AltText = product.Name,
                ImageUrl = i.ImageUrl,
                PublicId = i.PublicId
            }).ToArray();

            await Repository.AddRangeAsync(images);

            ProductImage thumbnailImage = new()
            {
                ProductId = product.Id,
                AltText = model.Name,
                ImageUrl = model.Thumbnail.ImageUrl,
                PublicId = model.Thumbnail.PublicId
            };

            await Repository.AddAsync(thumbnailImage);
            await Repository.SaveChangesAsync();

            ProductThumbnail thumbnail = new()
            {
                ProductId = product.Id,
                ImageId = thumbnailImage.Id,
                ImageOriginalId = images[model.ThumbnailOriginalImageIndex].Id
            };

            product.Thumbnail = thumbnail;

            if (model.Attributes.Count > 0) // ToDO Check if category has attributes
            {
                ICollection<ProductAttribute> attributes = model.Attributes
                    .Select(a => new ProductAttribute
                    {
                        Key = a.Key,
                        Value = a.Value,
                        ProductId = product.Id,
                        AttributeDefinitionId = a.AttributeDefinitionId
                    })
                    .ToArray();

                await Repository.AddRangeAsync(attributes);
            }

            await Repository.SaveChangesAsync();

            return ServiceResult.Ok();
        }
        catch (Exception)
        {
            return ServiceResult.Failure();
        }
    }

    public async Task<ServiceResult<EditProductFormViewModel>> GetProductForEditAsync(Guid productId)
    {
        Product? product = await Repository
            .AllReadonly<Product>()
            .IgnoreQueryFilters()
            .Where(p => !p.IsDeleted)
            .Include(p => p.Attributes)
                .ThenInclude(a => a.AttributeDefinition)
            .Include(p => p.ProductType)
            .Include(p => p.Images)
            .Include(p => p.Thumbnail)
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product is null) return ServiceResult<EditProductFormViewModel>.NotFound();

        EditProductFormViewModel model = new()
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            ProductTypeName = product.ProductType.Name,
            Weight = product.Weight,
            Images = product.Images
                .Where(i => i.Id != product.Thumbnail.ImageId)
                .Select(i => new ProductImageViewModel
                {
                    Id = i.Id,
                    Url = i.ImageUrl,
                    IsTitle = i.Id == product.Thumbnail.ImageOriginalId,
                })
                .ToArray(),
            Attributes = product.Attributes
                .Select(a => new AttributeValueForm
                {
                    AttributeDefinitionId = a.AttributeDefinitionId,
                    Label = a.AttributeDefinition.Label,
                    Key = a.Key,
                    Value = a.Value,
                    DataType = a.AttributeDefinition.DataType,
                    // HasPredefinedValue = a.AttributeDefinition.HasPredefinedValue, TODO implement predefined values
                    IsRequired = a.AttributeDefinition.IsRequired, // TODO Use this to validate the form
                })
                .ToArray()
        };

        return ServiceResult<EditProductFormViewModel>.Ok(model);
    }

    public async Task<ServiceResult> UpdateProductAsync(EditProductFormViewModel model)
    {
        Product? product = await Repository
            .All<Product>()
            .IgnoreQueryFilters()
            .Where(p => !p.IsDeleted)
            .Include(p => p.Attributes)
            .Include(p => p.ProductType)
            .FirstOrDefaultAsync(p => p.Id == model.Id);

        if (product is null) return ServiceResult<EditProductFormViewModel>.NotFound();

        product.Name = model.Name;
        product.Description = model.Description;
        product.Price = model.Price;
        product.StockQuantity = model.StockQuantity;
        product.Weight = model.Weight;

        for (int i = 0; i < model.Attributes.Count; i++)
            product.Attributes.ElementAt(i).Value = model.Attributes.ElementAt(i).Value;

        await Repository.SaveChangesAsync();

        await Cache.RemoveAsync(ProductDetailsKey(product.Id));
        await Cache.RemoveAsync(ProductCardKey(product.Id));

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteProductAsync(Guid productId)
    {
        try
        {
            Product? product = await Repository
                .All<Product>()
                .IgnoreQueryFilters()
                .Where(p => !p.IsDeleted)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product is null) return ServiceResult.NotFound();

            product.IsDeleted = true;
            await Repository.SaveChangesAsync();

            await Cache.RemoveAsync(ProductCardKey(product.Id));
            await Cache.RemoveAsync(ProductDetailsKey(product.Id));

            return ServiceResult.Ok();
        }
        catch (Exception)
        {
            return ServiceResult.Failure();
        }
    }

    public async Task<ICollection<ProductTypeAttributesDto>> GetProductTypeAttributesAsync(int productTypeId) =>
        await Repository
            .Where<ProductAttributeDefinition>(pad => pad.ProductTypeId == productTypeId)
            .Select(pad => new ProductTypeAttributesDto
            {
                AttributeDefinitionId = pad.Id,
                Key = pad.Key,
                Label = pad.Label,
                DataType = (int)pad.DataType,
                HasPredefinedValue = pad.HasPredefinedValue,
                IsRequired = pad.IsRequired
            })
            .ToArrayAsync();

    public async Task<ICollection<ProductListViewModel>> GetProductsListReadonlyAsync() =>
        await Repository
            .AllReadonly<Product>()
            .IgnoreQueryFilters()
            .Include(p => p.ProductType)
            .Include(p => p.Discounts)
            .Where(p => !p.IsDeleted)
            .Select(p => new ProductListViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                ProductTypeName = p.ProductType.Name,
                IsPublic = p.IsPublic,
                HasActiveDiscount = p.Discounts.Any(d => d.StartDate <= DateTime.Now && d.EndDate >= DateTime.Now)
            })
            .ToArrayAsync();

    public async Task<ServiceResult> ToggleProductPublicityAsync(Guid productId)
    {
        try
        {
            Product? product = await Repository
                .All<Product>()
                .IgnoreQueryFilters()
                .Where(p => !p.IsDeleted)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product is null) return ServiceResult.NotFound();

            product.IsPublic = !product.IsPublic;
            await Repository.SaveChangesAsync();

            return ServiceResult.Ok();
        }
        catch (Exception)
        {
            return ServiceResult.Failure();
        }
    }

    public async Task<ICollection<Guid>> GetImageIdsByProductId(Guid productId)
        => await Repository
                .WhereReadonly<ProductImage>(i => i.ProductId == productId)
                .IgnoreQueryFilters()
                .Select(i => i.Id)
                .ToArrayAsync();

    public async Task<ServiceResult<decimal>> GetProductPriceReadonlyAsync(Guid productId)
    {
        decimal? price = await Repository.WhereReadonly<Product>(p => p.Id == productId)
            .Select(p => (decimal?)p.Price)
            .FirstOrDefaultAsync();
        if (price is null)
            return ServiceResult<decimal>.NotFound(new Dictionary<string, string>
            {
                { "ProductId", "Продуктът не беше намерен!" }
            });

        return ServiceResult<decimal>.Ok(price.Value);
    }

    public async Task<ServiceResult> AddDiscountAsync(AddProductDiscountFormViewModel model)
    {
        if (!await Repository.AnyAsync<Product>(p => p.Id == model.ProductId))
            return ServiceResult.NotFound(new Dictionary<string, string>
            {
                { "ProductId", "Продуктът не беше намерен!" }
            });

        if (await Repository.AnyAsync<ProductDiscount>(pd => pd.ProductId == model.ProductId && pd.EndDate > DateTime.Now))
            return ServiceResult.Forbidden(new Dictionary<string, string>
            {
                { "ProductDiscount", "Вече има зададена промоция за този продукт!" }
            });

        ProductDiscount discount = new()
        {
            ProductId = model.ProductId,
            NewPrice = model.NewPrice,
            StartDate = model.StartDate,
            EndDate = model.EndDate
        };

        await Repository.AddAsync(discount);
        await Repository.SaveChangesAsync();

        await Cache.RemoveAsync(ProductDetailsKey(model.ProductId));
        await Cache.RemoveAsync(ProductCardKey(model.ProductId));

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> EndDiscountAsync(Guid productId)
    {
        ProductDiscount? discount = await Repository
            .All<ProductDiscount>()
            .FirstOrDefaultAsync(pd => pd.ProductId == productId && pd.EndDate > DateTime.Now);

        if (discount is null) return ServiceResult.NotFound();

        discount.EndDate = DateTime.Now;
        await Repository.SaveChangesAsync();

        await Cache.RemoveAsync(ProductDetailsKey(productId));
        await Cache.RemoveAsync(ProductCardKey(productId));
        await Cache.RemoveAsync(GlobalDiscountsStateIdKey());

        return ServiceResult.Ok();
    }
}