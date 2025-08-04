using CloudinaryDotNet.Actions;

namespace MHAuthorWebsite.Core.Admin.Contracts;

public interface ICloudinaryService
{
    Task<DeletionResult> DestroyAsync(string publicId);

    Task<DeletionResult> DestroyAsync(DeletionParams deletionParams);

    Task<ImageUploadResult> UploadAsync(ImageUploadParams uploadParams);
}