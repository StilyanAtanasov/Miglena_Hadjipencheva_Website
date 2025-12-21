using MHAuthorWebsite.Core.Models;

namespace MHAuthorWebsite.Core.Contracts.DataServices;

public interface IProductDataService
{
    Task<Product?> GetProductWithLikesForEditByIdAsync(Guid productId);
}