using MHAuthorWebsite.Core.Common.Utils;
using Microsoft.AspNetCore.Http;

namespace MHAuthorWebsite.Core.Contracts;

public interface ICloudinaryCommentImageService : IImageService
{
    Task<ServiceResult<ICollection<string>>> UploadCommentImagesAsync(ICollection<string> imageUrls);

    Task<ServiceResult<ICollection<string>>> UploadCommentImagesAsync(ICollection<IFormFile> images);
}