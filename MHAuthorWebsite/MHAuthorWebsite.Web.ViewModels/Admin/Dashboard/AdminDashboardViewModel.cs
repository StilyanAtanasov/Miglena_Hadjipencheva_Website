namespace MHAuthorWebsite.Web.ViewModels.Admin.Dashboard;

public class AdminDashboardViewModel
{
    public int UsersCount { get; set; }

    public ICollection<AdminDashboardProductsViewModel> ProductsList { get; set; } = null!;
}