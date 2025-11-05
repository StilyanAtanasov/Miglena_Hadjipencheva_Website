using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Dto;
using Microsoft.AspNetCore.Http;

namespace MHAuthorWebsite.Core.Contracts;

public interface IImageService
{
    Task<ServiceResult<ICollection<ImageUploadResultDto>>> UploadImagesAsync(ICollection<IFormFile> images, string folder, short width);

    Task<ServiceResult<ICollection<ImageUploadResultDto>>> UploadImagesAsync(ICollection<string> imageUrls, string folder, short width);

    /// <summary>
    ///     Uploads an image to the specified path.
    /// </summary>
    /// <param name="images">The image files to upload.</param>
    /// <param name="titleImageId">The ID of the title image.</param>
    /// <returns>The URL of the uploaded image.</returns>
    Task<ServiceResult<ICollection<ProductImageUploadResultDto>>> UploadImageWithPreviewAsync(ICollection<IFormFile> images,
        int titleImageId);

    /// <summary>
    ///     Deletes an image from the specified path.
    /// </summary>
    /// <param name="publicId">The public id of the image to delete.</param>
    Task<ServiceResult> DeleteImageAsync(string publicId);
}