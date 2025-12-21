using MHAuthorWebsite.Core.Dtos.ProductComment;

namespace MHAuthorWebsite.Core.Dtos.Product;

public class ProductDetailsDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string ProductTypeName { get; set; } = null!;

    public decimal Price { get; set; }

    public bool IsInStock { get; set; }

    public bool IsLiked { get; set; }

    public decimal AverageRating { get; set; }

    public int TotalBaseComments { get; set; }

    public Dictionary<int, int> CommentsCountByStarsRating { get; set; } = null!;

    public bool HasMoreComments { get; set; }

    public bool CanWriteMoreComments { get; set; }

    public bool IsRateLimitedForReplies { get; set; }

    public ProductDetailsDiscountDto? Discount { get; set; }

    public ICollection<ProductDetailsImageDto> Images { get; set; } = new HashSet<ProductDetailsImageDto>();

    public ICollection<ProductAttributeDetailsDto> Attributes { get; set; } = new HashSet<ProductAttributeDetailsDto>();

    public ICollection<ProductBaseCommentDto> Comments { get; set; } = new HashSet<ProductBaseCommentDto>();
}