namespace MHAuthorWebsite.Web.ViewModels.Product;

public class LikedProductsListViewModel
{
    public Guid DiscountStateId { get; set; }

    public ICollection<LikedProductViewModel> LikedProducts { get; set; } = new HashSet<LikedProductViewModel>();
}