using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Dto;
using MHAuthorWebsite.Data.Models.Enums;
using MHAuthorWebsite.Web.ViewModels.Product;
using MHAuthorWebsite.Web.ViewModels.ProductComment;

namespace MHAuthorWebsite.Core.Contracts;

public interface IProductCommentService
{
    Task<ServiceResult> AddCommentAsync(string userId, AddProductCommentViewModel model, ICollection<ProductCommentImagesUploadDto>? images);

    Task<ServiceResult<ICollection<ProductCommentReactionViewModel>>> ReactToComment(string userId, Guid commentId, CommentReaction reactionType);
}