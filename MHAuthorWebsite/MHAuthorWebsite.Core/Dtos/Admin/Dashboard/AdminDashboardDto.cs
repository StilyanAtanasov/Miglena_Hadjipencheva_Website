namespace MHAuthorWebsite.Core.Dtos.Admin.Dashboard;

public class AdminDashboardDto
{
    public int UsersCount { get; set; }

    public int ActiveUsersCount { get; set; }

    public int NewUsersCount { get; set; }

    public ICollection<AdminDashboardProductsDto> ProductsList { get; set; } = null!;
}