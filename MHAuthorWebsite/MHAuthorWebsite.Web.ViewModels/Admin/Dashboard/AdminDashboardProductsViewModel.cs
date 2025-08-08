namespace MHAuthorWebsite.Web.ViewModels.Admin.Dashboard;

public class AdminDashboardProductsViewModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public int LikesCount { get; set; }

    public int SoldCount { get; set; }
}