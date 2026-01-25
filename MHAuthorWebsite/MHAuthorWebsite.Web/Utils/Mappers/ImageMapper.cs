using MHAuthorWebsite.Core.Admin.Dto;

namespace MHAuthorWebsite.Web.Utils.Mappers;

public class ImageMapper
{
    public static async Task<UploadImageRequestDto> MapIFormFileToUploadImageRequestDtoAsync(
        IFormFile file)
    {
        MemoryStream memoryStream = new();
        await file.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        return new UploadImageRequestDto
        {
            Content = memoryStream,
            FileName = file.FileName,
            ContentType = file.ContentType
        };
    }

    public static async Task<ICollection<UploadImageRequestDto>> MapIFormFileCollectionToUploadImageRequestDtoAsync(
        ICollection<IFormFile> files)
    {
        List<UploadImageRequestDto> result = new(files.Count);

        foreach (IFormFile file in files)
        {
            MemoryStream memoryStream = new();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            result.Add(new UploadImageRequestDto
            {
                Content = memoryStream,
                FileName = file.FileName,
                ContentType = file.ContentType
            });
        }

        return result;
    }
}