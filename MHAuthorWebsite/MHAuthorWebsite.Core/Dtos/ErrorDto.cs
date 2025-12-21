namespace MHAuthorWebsite.Core.Dtos;

public class ErrorDto
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}