using MHAuthorWebsite.Core.Dto;
using MHAuthorWebsite.Web.ViewModels.Product;

namespace MHAuthorWebsite.Core.Admin.Dto;

public class AddProductDto
{
    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public decimal Weight { get; set; }

    public int ProductTypeId { get; set; }

    public ImageUploadResultDto Thumbnail { get; set; } = null!;

    public int ThumbnailOriginalImageIndex { get; set; }

    public ICollection<ImageUploadResultDto> ImageUrls { get; set; } = null!;

    public ICollection<AttributeValueForm> Attributes { get; set; } = new HashSet<AttributeValueForm>();
}