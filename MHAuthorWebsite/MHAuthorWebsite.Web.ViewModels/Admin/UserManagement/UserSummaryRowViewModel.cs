namespace MHAuthorWebsite.Web.ViewModels.Admin.UserManagement;

public class UserSummaryRowViewModel
{
    public string Id { get; set; } = null!;

    public string? Name { get; set; }

    public string? Email { get; set; }

    public bool IsActive { get; set; }

    public bool IsAdmin { get; set; }

    public bool IsBanned { get; set; }

    public bool IsDeleted { get; set; }
}