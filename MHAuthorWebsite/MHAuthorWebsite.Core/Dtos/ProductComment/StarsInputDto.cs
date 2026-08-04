namespace MHAuthorWebsite.Core.Dtos.ProductComment;

public class StarsInputDto
{
    public short MaxValue { get; set; }

    public string InputName { get; set; } = null!;

    public byte Rating { get; set; } = 0;
}