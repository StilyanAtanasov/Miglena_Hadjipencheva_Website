using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Dtos.ProductComment;

namespace MHAuthorWebsite.Core.Contracts;

public interface ICommentImageService : IImageService
{
    Task<ServiceResult<ICollection<ProductCommentImagesUploadDto>>> UploadCommentImagesAsync(
        ICollection<UploadImageRequestDto> images,
        CancellationToken cancellationToken = default);

    Task<ServiceResult> DeleteCommentImagesAsync(
        ICollection<string> publicIds,
        CancellationToken cancellationToken = default);
}
