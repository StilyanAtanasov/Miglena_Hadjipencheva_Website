using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Dtos.ProductComment;
using MHAuthorWebsite.Core.Models.Enums;

namespace MHAuthorWebsite.Core.Contracts;

public interface IProductCommentService
{
    Task<ServiceResult<ProductCommentDetailsDto>> GetCommentDetailsReadonlyAsync(Guid commentId, string? userId);

    Task<ServiceResult> AddCommentAsync(string userId, AddProductCommentDto model, ICollection<ProductCommentImagesUploadDto>? images);

    Task<ServiceResult<EditProductCommentDto>> GetCommentForEditReadonlyAsync(string userId, Guid commentId);

    Task<ServiceResult<ICollection<string>>> EditCommentAsync(string userId, EditProductCommentDto model,
        ICollection<ProductCommentImagesUploadDto>? newImages, ICollection<Guid>? removedImagesUrls);

    Task<ServiceResult<ICollection<ProductCommentReactionDto>>> ReactToComment(string userId, Guid commentId, CommentReaction reactionType);

    Task<ServiceResult<CommentPageDto>> LoadCommentsReadonlyAsync(Guid productId, int page, int? ratingFilter, string? userId);

    Task<ServiceResult<ReplyPageDto>> LoadRepliesReadonlyAsync(Guid productId, Guid commentId, int page, string? userId);

    Task<ServiceResult> DeleteCommentAsync(string userId, Guid commentId);

    Task<decimal> GetAverageRatingAsync(Guid productId);
}