using MHAuthorWebsite.Core.Contracts.DataServices;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using Microsoft.EntityFrameworkCore;

namespace MHAuthorWebsite.Data.DataServices;

public class ProductDataService : IProductDataService
{
    private readonly IApplicationRepository _repository;

    public ProductDataService(IApplicationRepository repository) => _repository = repository;

    public async Task<Product?> GetProductWithLikesForEditByIdAsync(Guid productId)
     => await _repository
         .All<Product>()
         .Include(p => p.Likes)
         .FirstOrDefaultAsync(p => p.Id == productId);
}