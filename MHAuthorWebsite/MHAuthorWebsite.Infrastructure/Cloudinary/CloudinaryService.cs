using CloudinaryDotNet.Actions;

namespace MHAuthorWebsite.Infrastructure.Cloudinary;

public class CloudinaryService : ICloudinaryService
{
    private readonly CloudinaryDotNet.Cloudinary _cloudinary;

    public CloudinaryService(CloudinaryDotNet.Cloudinary cloudinary) => _cloudinary = cloudinary;

    public async Task<DeletionResult> DestroyAsync(string publicId, CancellationToken cancellationToken = default)
        => await _cloudinary.DestroyAsync(new DeletionParams(publicId)).WaitAsync(cancellationToken);

    public async Task<DeletionResult> DestroyAsync(DeletionParams deletionParams, CancellationToken cancellationToken = default)
        => await _cloudinary.DestroyAsync(deletionParams).WaitAsync(cancellationToken);

    public async Task<ImageUploadResult> UploadAsync(ImageUploadParams uploadParams, CancellationToken cancellationToken = default)
        => await _cloudinary.UploadAsync(uploadParams).WaitAsync(cancellationToken);
}
