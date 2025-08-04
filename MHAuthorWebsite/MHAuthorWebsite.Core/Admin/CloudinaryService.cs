using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using MHAuthorWebsite.Core.Admin.Contracts;

namespace MHAuthorWebsite.Core.Admin;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(Cloudinary cloudinary) => _cloudinary = cloudinary;

    public async Task<DeletionResult> DestroyAsync(string publicId)
        => await _cloudinary.DestroyAsync(new DeletionParams(publicId));

    public async Task<DeletionResult> DestroyAsync(DeletionParams deletionParams)
        => await _cloudinary.DestroyAsync(deletionParams);

    public async Task<ImageUploadResult> UploadAsync(ImageUploadParams uploadParams)
        => await _cloudinary.UploadAsync(uploadParams);
}