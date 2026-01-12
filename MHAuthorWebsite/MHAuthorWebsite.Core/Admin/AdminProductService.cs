using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Admin.Contracts.DataServices;
using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Contracts.DataServices;
using MHAuthorWebsite.Core.Dtos.Admin.Product;
using MHAuthorWebsite.Core.Dtos.Product;
using MHAuthorWebsite.Core.Extensions;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using Microsoft.AspNetCore.Identity;
using static MHAuthorWebsite.GCommon.ApplicationRules.Application;
using static MHAuthorWebsite.GCommon.ApplicationRules.CacheKeys;
using AddProductDto = MHAuthorWebsite.Core.Admin.Dto.AddProductDto;

namespace MHAuthorWebsite.Core.Admin;

public class AdminProductService : ProductService, IAdminProductService
{
    private readonly IAdminProductDataService _adminProductDataService;

    public AdminProductService(IFastCacheService cacheService, IApplicationRepository repository,
        IGlobalCacheKeysManagementService globalCacheKeysManagementService,
        UserManager<ApplicationUser> userManager, IProductDataService productDataService,
        IAdminProductDataService adminProductDataService)
        : base(cacheService, productDataService, globalCacheKeysManagementService, repository, userManager)
        => _adminProductDataService = adminProductDataService;

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
                        AttributeDefinitionId = a.AttributeDefinitionId,
                        DisplayPosition = a.DisplayPosition
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

    public async Task<ServiceResult<EditProductDto>> GetProductForEditAsync(Guid productId)
    {
        Product? product = await _adminProductDataService.GetProductForEditByIdReadonlyAsync(productId);

        if (product is null) return ServiceResult<EditProductDto>.NotFound();

        EditProductDto model = new()
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
                .Select(i => new ProductImageDto
                {
                    Id = i.Id,
                    Url = i.ImageUrl,
                    IsTitle = i.Id == product.Thumbnail.ImageOriginalId
                })
                .ToArray(),
            Attributes = product.Attributes
                .Select(a => new AttributeValueDto
                {
                    AttributeDefinitionId = a.AttributeDefinitionId,
                    Label = a.AttributeDefinition.Label,
                    Key = a.Key,
                    Value = a.Value,
                    DisplayPosition = a.DisplayPosition,
                    DataType = a.AttributeDefinition.DataType,
                    // HasPredefinedValue = a.AttributeDefinition.HasPredefinedValue, TODO implement predefined values
                    IsRequired = a.AttributeDefinition.IsRequired, // TODO Use this to validate the form
                })
                .ToArray()
        };

        return ServiceResult<EditProductDto>.Ok(model);
    }

    public async Task<ServiceResult> UpdateProductAsync(EditProductDto model)
    {
        Product? product = await _adminProductDataService.GetProductForUpdateByIdAsync(model.Id);

        if (product is null) return ServiceResult<EditProductDto>.NotFound();

        product.Name = model.Name;
        product.Description = model.Description;
        product.Price = model.Price;
        product.StockQuantity = model.StockQuantity;
        product.Weight = model.Weight;

        for (int i = 0; i < model.Attributes.Count; i++)
        {
            AttributeValueDto attributeModel = model.Attributes.ElementAt(i);
            ProductAttribute attribute = product.Attributes.ElementAt(i);

            attribute.Value = attributeModel.Value;
            attribute.DisplayPosition = attributeModel.DisplayPosition;
        }

        await Repository.SaveChangesAsync();

        await Cache.RemoveAsync(ProductDetailsKey(product.Id));
        await Cache.RemoveAsync(ProductCardKey(product.Id));

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteProductAsync(Guid productId)
    {
        try
        {
            Product? product = await _adminProductDataService.GetNonDeletedProductByIdAsync(productId);

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

    public async Task<ICollection<ProductListItemDto>> GetProductsListReadonlyAsync()
        => await _adminProductDataService.GetProductsListReadonlyAsync();

    public async Task<ServiceResult> ToggleProductPublicityAsync(Guid productId)
    {
        try
        {
            Product? product = await _adminProductDataService.GetNonDeletedProductByIdAsync(productId);

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
        => await _adminProductDataService.GetImageIdsByProductId(productId);

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

    public async Task<ServiceResult> AddDiscountAsync(AddProductDiscountDto model)
    {
        if (!await Repository.AnyAsync<Product>(p => p.Id == model.ProductId))
            return ServiceResult.NotFound(new Dictionary<string, string>
            {
                { "ProductId", "Продуктът не беше намерен!" }
            });

        if (await Repository.AnyAsync<ProductDiscount>(pd => pd.ProductId == model.ProductId && pd.EndDate > DateTime.UtcNow))
            return ServiceResult.Forbidden(new Dictionary<string, string>
            {
                { "ProductDiscount", "Вече има зададена промоция за този продукт!" }
            });

        TimeZoneInfo bgZone = TimeZoneInfo.FindSystemTimeZoneById(DefaultTimeZoneId);
        ProductDiscount discount = new()
        {
            ProductId = model.ProductId,
            NewPrice = model.NewPrice,
            StartDate = TimeZoneInfo.ConvertTimeToUtc(model.StartDate, bgZone),
            EndDate = TimeZoneInfo.ConvertTimeToUtc(model.EndDate, bgZone)
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
            .Where<ProductDiscount>(pd => pd.ProductId == productId && pd.EndDate > DateTime.UtcNow)
            .FirstOrDefaultAsync();

        if (discount is null) return ServiceResult.NotFound();

        discount.EndDate = DateTime.UtcNow;
        await Repository.SaveChangesAsync();

        await Cache.RemoveAsync(ProductDetailsKey(productId));
        await Cache.RemoveAsync(ProductCardKey(productId));
        await Cache.RemoveAsync(GlobalDiscountsStateIdKey());

        return ServiceResult.Ok();
    }
}