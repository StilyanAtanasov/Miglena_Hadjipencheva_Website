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
using MHAuthorWebsite.Core.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using static MHAuthorWebsite.GCommon.ApplicationRules.Application;
using static MHAuthorWebsite.GCommon.ApplicationRules.CacheKeys;
using AddProductDto = MHAuthorWebsite.Core.Admin.Dto.AddProductDto;

namespace MHAuthorWebsite.Core.Admin;

public class AdminProductService : ProductService, IAdminProductService
{
    private readonly IAdminProductDataService _adminProductDataService;
    private readonly ILogger<AdminProductService> _logger;

    public AdminProductService(
        IFastCacheService cacheService,
        IApplicationRepository repository,
        IGlobalCacheKeysManagementService globalCacheKeysManagementService,
        UserManager<ApplicationUser> userManager,
        IProductDataService productDataService,
        IAdminProductDataService adminProductDataService,
        ILogger<ProductService> baseLogger,
        ILogger<AdminProductService> logger)
        : base(cacheService, productDataService, globalCacheKeysManagementService, repository, userManager, baseLogger)
    {
        _adminProductDataService = adminProductDataService;
        _logger = logger;
    }

    public async Task<ServiceResult> AddProductAsync(AddProductDto model)
    {
        try
        {
            DateTime now = DateTime.UtcNow;

            Product product = new()
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                Currency = Currency,
                ProductTypeId = model.ProductTypeId,
                StockQuantity = model.StockQuantity,
                Weight = model.Weight,
                UpdatedOn = now
            };

            ProductImage[] images = model.ImageUrls
                .Select(i => new ProductImage
                {
                    AltText = product.Name,
                    ImageUrl = i.ImageUrl,
                    PublicId = i.PublicId
                })
                .ToArray();

            ProductImage thumbnailImage = new()
            {
                AltText = model.Name,
                ImageUrl = model.Thumbnail.ImageUrl,
                PublicId = model.Thumbnail.PublicId
            };

            product.Images = images.Append(thumbnailImage).ToArray();

            ProductThumbnail thumbnail = new()
            {
                Image = thumbnailImage,
                ImageOriginal = images[model.ThumbnailOriginalImageIndex]
            };

            product.Thumbnail = thumbnail;


            int[] definitionIds = model.Attributes.Select(a => a.AttributeDefinitionId).Distinct().ToArray();

            var productTypeDefinitions = await Repository
                .WhereReadonly<ProductAttributeDefinition>(pad => pad.ProductTypeId == model.ProductTypeId)
                .Select(pad => new
                {
                    pad.Id,
                    pad.Label,
                    pad.IsRequired
                })
                .ToArrayAsync();

            foreach (var definition in productTypeDefinitions)
                if (!definitionIds.Contains(definition.Id) && definition.IsRequired)
                    return ServiceResult.Failure(new Dictionary<string, string>
                    {
                        { "Attributes", $"Липсва задължителен атрибут: {definition.Label}!" }
                    });

            if (model.Attributes.Count > 0)
            {
                ProductAttributeOption[] attributeOptionsForProduct = await Repository
                    .WhereReadonly<ProductAttributeOption>(pao => definitionIds.Contains(pao.AttributeDefinitionId))
                    .ToArrayAsync();

                bool hasInvalidAttributeOptions = model.Attributes
                    .Where(a => a.DataType == AttributeDataType.Dropdown)
                    .Select(a => new
                    {
                        a.ProductAttributeOptionId,
                        a.AttributeDefinitionId
                    })
                    .Any(a =>
                    {
                        ProductAttributeOption? option = attributeOptionsForProduct
                            .FirstOrDefault(pao => pao.Id == a.ProductAttributeOptionId);

                        return option is null || option.AttributeDefinitionId != a.AttributeDefinitionId;
                    });

                if (hasInvalidAttributeOptions) return ServiceResult.Failure(new Dictionary<string, string>
                {
                    { "Attributes", "Невалидна опция за атрибут!" }
                });

                ICollection<ProductAttribute> attributes = model.Attributes
                    .Select(a => new ProductAttribute
                    {
                        Value = a.Value,
                        AttributeDefinitionId = a.AttributeDefinitionId,
                        DisplayPosition = a.DisplayPosition,
                        ProductAttributeOptionId = a.ProductAttributeOptionId
                    })
                    .ToArray();

                product.Attributes = attributes;
            }

            await Repository.AddAsync(product);
            await Repository.SaveChangesAsync();

            _logger.LogInformation("Admin added new product: {ProductName}", model.Name);

            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding product: {ProductName}", model.Name);
            return ServiceResult.Failure(new Dictionary<string, string>() { { "DBError", "Грешка при добавяне на продукта в базата!" } });
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
                    Key = a.AttributeDefinition.Key,
                    Value = a.Value == null && a.ProductAttributeOptionId != null
                        ? a.ProductAttributeOption!.Value
                        : a.Value,
                    DisplayPosition = a.DisplayPosition,
                    DataType = a.AttributeDefinition.DataType,
                    IsRequired = a.AttributeDefinition.IsRequired,
                    ProductAttributeOptionId = a.ProductAttributeOption?.Id ?? null,
                    PredefinedValues = a.AttributeDefinition.ProductAttributeOptions
                        .Select(pao => new AttributeOptionDto
                        {
                            Id = pao.Id,
                            Value = pao.Value
                        })
                        .ToArray()
                })
                .ToArray()
        };

        _logger.LogInformation("Admin retrieved product {ProductId} for edit.", productId);
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
        product.UpdatedOn = DateTime.UtcNow;

        for (int i = 0; i < model.Attributes.Count; i++)
        {
            AttributeValueDto attributeModel = model.Attributes.ElementAt(i);
            ProductAttribute attribute = product.Attributes.ElementAt(i);

            attribute.Value = attributeModel.Value;
            attribute.DisplayPosition = attributeModel.DisplayPosition;
            attribute.ProductAttributeOptionId = attributeModel.ProductAttributeOptionId;
        }

        await Repository.SaveChangesAsync();

        await Cache.RemoveAsync(ProductDetailsKey(product.Id));
        await Cache.RemoveAsync(ProductCardKey(product.Id));

        _logger.LogInformation("Admin updated product {ProductId}.", model.Id);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteProductAsync(Guid productId)
    {
        try
        {
            Product? product = await _adminProductDataService.GetNonDeletedProductByIdAsync(productId);

            if (product is null) return ServiceResult.NotFound();

            product.IsDeleted = true;
            product.UpdatedOn = DateTime.UtcNow;
            await Repository.SaveChangesAsync();

            await Cache.RemoveAsync(ProductCardKey(product.Id));
            await Cache.RemoveAsync(ProductDetailsKey(product.Id));

            _logger.LogInformation("Admin deleted product {ProductId}.", productId);

            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", productId);
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
                IsRequired = pad.IsRequired,
                PredefinedValues = pad.ProductAttributeOptions
                    .Select(pao => new AttributeOptionDto
                    {
                        Id = pao.Id,
                        Value = pao.Value
                    })
                    .ToArray()
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
            product.UpdatedOn = DateTime.UtcNow;
            await Repository.SaveChangesAsync();

            _logger.LogInformation("Admin toggled publicity for product {ProductId}.", productId);

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

        _logger.LogInformation("Admin retrieved price for product {ProductId}.", productId);
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
            Currency = Currency,
            StartDate = TimeZoneInfo.ConvertTimeToUtc(model.StartDate, bgZone),
            EndDate = TimeZoneInfo.ConvertTimeToUtc(model.EndDate, bgZone)
        };

        await Repository.AddAsync(discount);
        await Repository.SaveChangesAsync();

        await Cache.RemoveAsync(ProductDetailsKey(model.ProductId));
        await Cache.RemoveAsync(ProductCardKey(model.ProductId));

        _logger.LogInformation("Admin added discount for product {ProductId}. New Price: {NewPrice}", model.ProductId, model.NewPrice);
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

        _logger.LogInformation("Admin ended discount for product {ProductId}.", productId);
        return ServiceResult.Ok();
    }
}
