using CloudinaryDotNet.Actions;

namespace MHAuthorWebsite.Core.Admin.Contracts;

public interface ICloudinaryService
{
    Task<DeletionResult> DestroyAsync(string publicId, CancellationToken cancellationToken = default);

    Task<DeletionResult> DestroyAsync(DeletionParams deletionParams, CancellationToken cancellationToken = default);

    Task<ImageUploadResult> UploadAsync(ImageUploadParams uploadParams, CancellationToken cancellationToken = default);
}