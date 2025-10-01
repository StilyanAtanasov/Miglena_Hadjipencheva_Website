namespace MHAuthorWebsite.Web.ViewModels.Product;
public class ProductCommentViewModel
{
    public short Rating { get; set; }

    public string Text { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public DateTime Date { get; set; }

    public bool VerifiedPurchase { get; set; }
}