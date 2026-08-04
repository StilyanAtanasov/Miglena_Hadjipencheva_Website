namespace MHAuthorWebsite.Core.Dtos.Admin.Dashboard;

public class AdminDashboardProductsDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public int LikesCount { get; set; }

    public int SoldCount { get; set; }
}