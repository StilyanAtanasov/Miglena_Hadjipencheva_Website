using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Data.Shared;
using MHAuthorWebsite.Web.ViewModels.Product;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MHAuthorWebsite.Core.Admin;

public class AdminProductService : ProductService, IAdminProductService
{
    private readonly IApplicationRepository _repository;

    public AdminProductService(IApplicationRepository repository, UserManager<IdentityUser> userManager)
    : base(repository, userManager)
        => _repository = repository;

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
            };

            await _repository.AddAsync(product);
            await _repository.SaveChangesAsync();

            for (int i = 0; i < model.ImageUrls.Count; i++)
            {
                ImageUploadResultDto imageResult = model.ImageUrls.ElementAt(i);

                Image image = new()
                {
                    ProductId = product.Id,
                    AltText = product.Name, // TODO Probably use the image title
                    ImageUrl = imageResult.OriginalUrl,
                    ThumbnailUrl = imageResult.PreviewUrl,
                    PublicId = imageResult.PublicId,
                    IsThumbnail = imageResult.IsThumbnail
                };

                await _repository.AddAsync(image);
            }
            await _repository.SaveChangesAsync();

            if (model.Attributes.Any())
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

    public async Task<ServiceResult<EditProductFormViewModel>> GetProductForEditAsync(Guid productId)
    {
        Product? product = await _repository
            .AllReadonly<Product>()
            .IgnoreQueryFilters()
            .Where(p => !p.IsDeleted)
            .Include(p => p.Attributes)
                .ThenInclude(a => a.AttributeDefinition)
            .Include(p => p.ProductType)
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
        Product? product = await _repository
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

        for (int i = 0; i < model.Attributes.Count; i++)
            product.Attributes.ElementAt(i).Value = model.Attributes.ElementAt(i).Value;

        await _repository.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteProductAsync(Guid productId)
    {
        try
        {
            Product? product = await _repository
                .All<Product>()
                .IgnoreQueryFilters()
                .Where(p => !p.IsDeleted)
                .FirstOrDefaultAsync(p => p.Id == productId);

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

    public async Task<ICollection<ProductTypeAttributesDto>> GetProductTypeAttributesAsync(int productTypeId) =>
        await _repository
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
}