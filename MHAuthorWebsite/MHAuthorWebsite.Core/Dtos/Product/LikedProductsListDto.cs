namespace MHAuthorWebsite.Core.Dtos.Product;

public class LikedProductsListDto
{
    public Guid DiscountStateId { get; set; }

    public ICollection<LikedProductDto> LikedProducts { get; set; } = new HashSet<LikedProductDto>();
}