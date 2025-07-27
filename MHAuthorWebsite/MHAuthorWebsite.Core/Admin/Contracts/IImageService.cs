using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Common.Utils;
using Microsoft.AspNetCore.Http;

namespace MHAuthorWebsite.Core.Admin.Contracts;

public interface IImageService
{
    /// <summary>
    ///     Uploads an image to the specified path.
    /// </summary>
    /// <param name="images">The image files to upload.</param>
    /// <param name="titleImageId">The ID of the title image.</param>
    /// <param name="productId">The ID of the product.</param>
    /// <returns>The URL of the uploaded image.</returns>
    Task<ServiceResult<ICollection<ImageUploadResultDto>>> UploadImageWithPreviewAsync(ICollection<IFormFile> images, int? titleImageId, Guid? productId = null);

    /// <summary>
    ///     Deletes an image from the specified path.
    /// </summary>
    /// <param name="publicId">The public id of the image to delete.</param>
    Task<ServiceResult> DeleteImageAsync(string publicId);

    /// <summary>
    /// Deletes a product image identified by the specified image ID.
    /// </summary>
    /// <remarks>This method performs an asynchronous operation to delete a product image. Ensure that the
    /// provided <paramref name="imageId"/>  corresponds to an existing image. The operation may fail if the image does
    /// not exist or if there are constraints preventing deletion.</remarks>
    /// <param name="imageId">The unique identifier of the product image to delete. Must not be an empty <see cref="Guid"/>.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The result contains a <see
    /// cref="ServiceResult"/>  indicating the success or failure of the operation.</returns>
    Task<ServiceResult> DeleteProductImageByIdAsync(Guid imageId);

    /// <summary>
    /// Updates the title image of a specified product.
    /// </summary>
    /// <remarks>This method updates the title image for the specified product by associating it with the
    /// provided image ID. Ensure that both the product and the image exist before calling this method.</remarks>
    /// <param name="productId">The unique identifier of the product whose title image is to be updated.</param>
    /// <param name="newTitleImageId">The unique identifier of the new title image to associate with the product.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task<ServiceResult> UpdateProductTitleImageAsync(Guid productId, Guid newTitleImageId);
}