namespace MHAuthorWebsite.Core.Dtos.Product;

public class ProductImageDto
{
    public Guid Id { get; set; }

    public string Url { get; set; } = null!;

    public bool IsTitle { get; set; }
}