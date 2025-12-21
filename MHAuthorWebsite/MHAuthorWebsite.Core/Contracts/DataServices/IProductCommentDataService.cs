using MHAuthorWebsite.Core.Models;

namespace MHAuthorWebsite.Core.Contracts.DataServices;

public interface IProductCommentDataService
{
    Task<decimal> GetAverageRatingAsync(Guid productId);

    Task<ProductComment?> GetCommentForReactionAsync(Guid commentId);

    Task<ProductComment?> GetCommentForEditReadonlyAsync(Guid commentId, string userId);

    Task<ProductComment?> GetCommentForRepliesLoadReadonlyAsync(Guid productId, Guid commentId);

    Task<Product?> GetProductForCommentsLoadAsync(Guid productId);

    Task<Product?> GetProductWithOrdersAndCommentsAsync(Guid productId);

    Task<ProductComment?> GetCommentForDeletionAsync(Guid commentId, string userId);
}