using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Dto;
using Microsoft.AspNetCore.Http;

namespace MHAuthorWebsite.Core.Contracts;

public interface ICommentImageService : IImageService
{
    Task<ServiceResult<ICollection<ProductCommentImagesUploadDto>>> UploadCommentImagesAsync(ICollection<string> imageUrls);

    Task<ServiceResult<ICollection<ProductCommentImagesUploadDto>>> UploadCommentImagesAsync(ICollection<IFormFile> images);
}