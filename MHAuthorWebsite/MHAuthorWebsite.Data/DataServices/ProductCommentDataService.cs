using MHAuthorWebsite.Core.Contracts.DataServices;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using Microsoft.EntityFrameworkCore;

namespace MHAuthorWebsite.Data.DataServices;

public class ProductCommentDataService : IProductCommentDataService
{
    private readonly IApplicationRepository _repository;

    public ProductCommentDataService(IApplicationRepository repository) => _repository = repository;

    public async Task<decimal> GetAverageRatingAsync(Guid productId)
    => await _repository
        .AllReadonly<ProductComment>()
        .Where(c => c.ProductId == productId)
        .AverageAsync(c => (decimal?)c.Rating) ?? 0m;

    public async Task<ProductComment?> GetCommentForReactionAsync(Guid commentId)
     => await _repository
         .All<ProductComment>()
         .Include(c => c.Reactions)
         .FirstOrDefaultAsync(c => c.Id == commentId);

    public async Task<ProductComment?> GetCommentForEditReadonlyAsync(Guid commentId, string userId)
     => await _repository
         .All<ProductComment>()
         .AsNoTracking()
         .Include(c => c.Images)
         .FirstOrDefaultAsync(c => c.Id == commentId && c.UserId == userId);

    public async Task<ProductComment?> GetCommentForEditAsync(Guid commentId, string userId)
        => await _repository
            .All<ProductComment>()
            .Include(c => c.Images)
            .FirstOrDefaultAsync(c => c.Id == commentId && c.UserId == userId);

    public async Task<ProductComment?> GetCommentForRepliesLoadReadonlyAsync(Guid commentId, Guid productId)
     => await _repository
         .WhereReadonly<ProductComment>(pc => pc.ProductId == productId && pc.Id == commentId)
         .Include(pc => pc.Replies)
         .ThenInclude(r => r.Reactions)
         .Include(pc => pc.Replies)
         .ThenInclude(pc => pc.User)
         .Include(pc => pc.Replies)
         .ThenInclude(pc => pc.ParentReply)
         .ThenInclude(pr => pr!.User)
         .FirstOrDefaultAsync();

    public async Task<Product?> GetProductForCommentsLoadAsync(Guid productId)
     => await _repository
         .AllReadonly<Product>()
         .Include(p => p.Comments)
            .ThenInclude(c => c.User)
         .Include(p => p.Comments)
            .ThenInclude(c => c.Replies)
                .ThenInclude(c => c.User)
         .Include(p => p.Comments)
            .ThenInclude(c => c.Images)
         .Include(p => p.Comments)
            .ThenInclude(c => c.Reactions)
         .Include(p => p.Comments)
            .ThenInclude(c => c.Replies)
                .ThenInclude(c => c.ParentReply)
                    .ThenInclude(r => r!.User)
         .FirstOrDefaultAsync(p => p.Id == productId);

    public async Task<Product?> GetProductWithOrdersAndCommentsAsync(Guid productId)
     => await _repository
         .All<Product>()
         .Include(p => p.Orders)
         .ThenInclude(op => op.Order)
         .Include(p => p.Comments)
         .FirstOrDefaultAsync(p => p.Id == productId);

    public async Task<ProductComment?> GetCommentForDeletionAsync(Guid commentId, string userId)
     => await _repository
         .All<ProductComment>()
         .Include(c => c.Replies)
         .FirstOrDefaultAsync(c => c.Id == commentId && c.UserId == userId);
}