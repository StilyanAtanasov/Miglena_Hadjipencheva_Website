using System.Text.Json.Serialization;

namespace MHAuthorWebsite.Web.Dto.Product;

public class ProductImagesJsonDto
{
    [JsonPropertyName("existing")]
    public ProductExistingImageJsonDto[] Existing { get; set; } = null!;

    [JsonPropertyName("added")]
    public bool[] Added { get; set; } = null!;

    [JsonPropertyName("deletedIds")]
    public Guid[] Deleted { get; set; } = null!;
}