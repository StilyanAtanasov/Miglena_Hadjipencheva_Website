using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Dtos.Images;


namespace MHAuthorWebsite.Core.Contracts;

public interface IImageService
{
    Task<ServiceResult<ICollection<ImageUploadResultDto>>> UploadImagesAsync(ICollection<UploadImageRequestDto> images, string folder, short width, CancellationToken cancellationToken = default);

    Task<ServiceResult<ICollection<ImageUploadResultDto>>> UploadImagesAsync(ICollection<string> imageUrls, string folder, short width, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Uploads an image to the specified path.
    /// </summary>
    /// <param name="images">The image files to upload.</param>
    /// <param name="titleImageId">The ID of the title image.</param>
    /// <returns>The URL of the uploaded image.</returns>
    Task<ServiceResult<ICollection<ProductImageUploadResultDto>>> UploadImageWithPreviewAsync(ICollection<UploadImageRequestDto> images,
        int titleImageId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes an image from the specified path.
    /// </summary>
    /// <param name="publicId">The public id of the image to delete.</param>
    Task<ServiceResult> DeleteImageAsync(string publicId, CancellationToken cancellationToken = default);

    Task<ServiceResult> DeleteImagesAsync(ICollection<string> publicIds, CancellationToken cancellationToken = default);
}