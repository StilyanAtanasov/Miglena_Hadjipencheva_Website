namespace MHAuthorWebsite.Core.Dtos.Error;

public class HandleErrorDto
{
    public Exception? Exception { get; set; }

    public string Path { get; set; } = null!;

    public string RequestId { get; set; } = null!;

    public string? UserId { get; set; }

    public string UserNameIdentifier { get; set; } = null!;

    public string Method { get; set; } = null!;
}