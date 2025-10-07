using CloudinaryDotNet.Actions;
using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Data.Models.Enums;
using MHAuthorWebsite.Web.ViewModels.Product;
using System.Linq.Expressions;

namespace MHAuthorWebsite.Core.Contracts;

public interface IProductService
{
    Task<ICollection<ProductCardViewModel>> GetAllProductCardsReadonlyAsync(string? userId, int page, (bool descending, Expression<Func<Product, object>>? expression) sortType);

    Task<int> GetAllProductsCountAsync();

    Task<ServiceResult<ProductDetailsViewModel>> GetProductDetailsReadonlyAsync(Guid productId, string? userId);

    Task<ICollection<LikedProductViewModel>> GetLikedProductsReadonlyAsync(string userId);

    Task<ServiceResult> ToggleLikeProduct(string userId, Guid productId);

    Task<ServiceResult> AddCommentAsync(string userId, AddProductCommentViewModel model, ICollection<ImageUploadResultDto> images);

    Task<ServiceResult<ICollection<ProductCommentReactionViewModel>>> ReactToComment(string userId, Guid commentId, CommentReaction reactionType);
}