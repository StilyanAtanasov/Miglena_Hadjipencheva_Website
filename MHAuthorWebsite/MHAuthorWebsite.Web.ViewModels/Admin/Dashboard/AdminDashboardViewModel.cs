namespace MHAuthorWebsite.Web.ViewModels.Admin.Dashboard;

public class AdminDashboardViewModel
{
    public int UsersCount { get; set; }

    public int ActiveUsersCount { get; set; }

    public int NewUsersCount { get; set; }

    public ICollection<AdminDashboardProductsViewModel> ProductsList { get; set; } = null!;
}