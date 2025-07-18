using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Data.Shared;
using MHAuthorWebsite.Web.ViewModels.Product;
using Microsoft.AspNetCore.Identity;

namespace MHAuthorWebsite.Core.Admin;

public class AdminProductService : ProductService, IAdminProductService
{
    private readonly IApplicationRepository _repository;

    public AdminProductService(IApplicationRepository repository, UserManager<IdentityUser> userManager)
    : base(repository, userManager)
        => _repository = repository;

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