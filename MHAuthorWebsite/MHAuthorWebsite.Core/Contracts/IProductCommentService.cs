using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Dto;
using MHAuthorWebsite.Data.Models.Enums;
using MHAuthorWebsite.Web.ViewModels.ProductComment;

namespace MHAuthorWebsite.Core.Contracts;

public interface IProductCommentService
{
    Task<ServiceResult<ProductCommentDetailsViewModel>> GetCommentDetailsReadonlyAsync(Guid commentId, string? userId);

    Task<ServiceResult> AddCommentAsync(string userId, AddProductCommentViewModel model, ICollection<ProductCommentImagesUploadDto>? images);

    Task<ServiceResult<EditProductCommentViewModel>> GetCommentForEditReadonlyAsync(string userId, Guid commentId);

    Task<ServiceResult<ICollection<string>>> EditCommentAsync(string userId, EditProductCommentViewModel model,
        ICollection<ProductCommentImagesUploadDto>? newImages, ICollection<Guid>? removedImagesUrls);

    Task<ServiceResult<ICollection<ProductCommentReactionViewModel>>> ReactToComment(string userId, Guid commentId, CommentReaction reactionType);

    Task<ServiceResult<CommentPageViewModel>> LoadCommentsReadonlyAsync(Guid productId, int page, int? ratingFilter, string? userId);

    Task<ServiceResult<ReplyPageViewModel>> LoadRepliesReadonlyAsync(Guid productId, Guid commentId, int page, string? userId);

    Task<ServiceResult> DeleteCommentAsync(string userId, Guid commentId);

    Task<decimal> GetAverageRatingAsync(Guid productId);
}