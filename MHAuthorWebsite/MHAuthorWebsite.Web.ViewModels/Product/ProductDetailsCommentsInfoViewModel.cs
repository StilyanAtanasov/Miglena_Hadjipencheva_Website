using MHAuthorWebsite.Web.ViewModels.ProductComment;

namespace MHAuthorWebsite.Web.ViewModels.Product;

public class ProductDetailsCommentsInfoViewModel
{
    public decimal AverageRating { get; set; }

    public int TotalBaseComments { get; set; }

    public bool HasMoreComments { get; set; }

    public ICollection<StarCountViewModel> CommentsCountByStarsRating { get; set; } = new HashSet<StarCountViewModel>();

    public ICollection<ProductBaseCommentGeneralInfoViewModel> Comments { get; set; } = new HashSet<ProductBaseCommentGeneralInfoViewModel>();
}