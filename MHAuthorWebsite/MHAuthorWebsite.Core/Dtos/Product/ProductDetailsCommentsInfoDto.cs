using MHAuthorWebsite.Core.Dtos.ProductComment;

namespace MHAuthorWebsite.Core.Dtos.Product;

public class ProductDetailsCommentsInfoDto
{
    public decimal AverageRating { get; set; }

    public int TotalBaseComments { get; set; }

    public bool HasMoreComments { get; set; }

    public ICollection<StarCountDto> CommentsCountByStarsRating { get; set; } = new HashSet<StarCountDto>();

    public ICollection<ProductBaseCommentGeneralInfoDto> Comments { get; set; } = new HashSet<ProductBaseCommentGeneralInfoDto>();
}