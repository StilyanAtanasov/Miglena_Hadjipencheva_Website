namespace MHAuthorWebsite.Core.Admin.Dto;

public class UploadImageRequestDto
{
    public Stream Content { get; init; } = default!;

    public string FileName { get; init; } = default!;

    public string ContentType { get; init; } = default!;
}