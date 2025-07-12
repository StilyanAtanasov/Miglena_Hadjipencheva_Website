using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dto;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Data.Shared;
using MHAuthorWebsite.Web.ViewModels.Product;
using Microsoft.EntityFrameworkCore;

namespace MHAuthorWebsite.Core;

public class ProductService : IProductService
{
    private readonly IApplicationRepository _repository;

    public ProductService(IApplicationRepository repository) => _repository = repository;

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
}