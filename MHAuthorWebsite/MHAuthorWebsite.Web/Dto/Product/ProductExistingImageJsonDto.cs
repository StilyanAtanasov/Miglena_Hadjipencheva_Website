using System.Text.Json.Serialization;

namespace MHAuthorWebsite.Web.Dto.Product;

public class ProductExistingImageJsonDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = null!;

    [JsonPropertyName("isTitle")]
    public bool IsTitle { get; set; }
}